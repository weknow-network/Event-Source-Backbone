using Microsoft.Extensions.Logging;

using Polly;

using StackExchange.Redis;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Private;

using static System.Math;

using static Weknow.EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;

// TODO: Poly policies, multiple levels,
// TODO: Claim from other inactive consumers

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    internal class RedisConsumerChannel : IConsumerChannelProvider
    {
        private const int MAX_DELAY = 5000;

        private readonly ILogger _logger;
        private readonly RedisConsumerChannelSetting _setting;
        private static int _index;
        private const string CONNECTION_NAME_PATTERN = "Event_Source_Consumer_{0}";
        private readonly RedisClientFactory _redisClientFactory;
        private readonly IConsumerStorageStrategy _defaultStorageStrategy;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="setting">The setting.</param>
        /// <param name="endpointEnvKey">The endpoint env key.</param>
        /// <param name="passwordEnvKey">The password env key.</param>
        public RedisConsumerChannel(
            ILogger logger,
            RedisConsumerChannelSetting setting,
            string endpointEnvKey,
            string passwordEnvKey)
        {
            _logger = logger;
            _setting = setting;
            string name = string.Format(
                                    CONNECTION_NAME_PATTERN,
                                    Interlocked.Increment(ref _index));
            _redisClientFactory = new RedisClientFactory(
                                                logger,
                                                name,
                                                RedisUsageIntent.Read,
                                                setting.RedisConfiguration,
                                                endpointEnvKey, passwordEnvKey);
            _defaultStorageStrategy = new RedisHashStorageStrategy(_redisClientFactory);
        }

        #endregion // Ctor

        #region SubsribeAsync

        /// <summary>
        /// Subscribe to the channel for specific metadata.
        /// </summary>
        /// <param name="plan">The consumer plan.</param>
        /// <param name="func">The function.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// When completed
        /// </returns>
        public async ValueTask SubsribeAsync(
                    IConsumerPlan plan,
                    Func<Announcement, IAck, ValueTask> func,
                    IEventSourceConsumerOptions options,
                    CancellationToken cancellationToken)    
        {
            while (!cancellationToken.IsCancellationRequested)
            { // connection errors
                IDatabaseAsync db = await _redisClientFactory.GetDbAsync();

                if (plan.Shard != string.Empty)
                    await SubsribeShardAsync(db, plan, func, options, cancellationToken);
                else
                    await SubsribePartitionAsync(db, plan, func, options, cancellationToken);

            }
        }

        #endregion // SubsribeAsync

        #region SubsribePartitionAsync

        /// <summary>
        /// Subscribe to all shards under a partition.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="plan">The consumer plan.</param>
        /// <param name="func">The function.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// When completed
        /// </returns>
        private async ValueTask SubsribePartitionAsync(
                    IDatabaseAsync db,
                    IConsumerPlan plan,
                    Func<Announcement, IAck, ValueTask> func,
                    IEventSourceConsumerOptions options,
                    CancellationToken cancellationToken)
        {
            var subscriptions = new Queue<Task>();
            int delay = 1;
            string partition = plan.Partition;
            int partitionSplit = partition.Length + 1;
            while (!cancellationToken.IsCancellationRequested)
            {   // loop for error cases
                try
                {
                    // infinite until cancellation
                    var keys = _redisClientFactory.GetKeysUnsafeAsync(
                                                            pattern: $"{partition}:*")
                                                    .WithCancellation(cancellationToken);
                    // TODO: [bnaya 2020-10] seem like memory leak, re-subscribe to same shard 
                    await foreach (string key in keys)
                    {
                        string shard = key.Substring(partitionSplit);
                        IConsumerPlan p = plan.WithShard(shard);
                        Task subscription = SubsribeShardAsync(db, plan, func, options, cancellationToken);
                        subscriptions.Enqueue(subscription);
                    }

                    break;
                }
                catch (Exception ex)
                {
                    plan.Logger.LogError(ex, "Partition subscription");
                    await DelayIfRetry();
                }
            }

            await Task.WhenAll(subscriptions);

            #region DelayIfRetry

            async Task DelayIfRetry()
            {
                await Task.Delay(delay, cancellationToken);
                delay *= Max(delay, 2);
                delay = Min(MAX_DELAY, delay);
            }

            #endregion // DelayIfRetry

        }

        #endregion // SubsribePartitionAsync

        #region SubsribeShardAsync

        /// <summary>
        /// Subscribe to specific shard.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="plan">The consumer plan.</param>
        /// <param name="func">The function.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task SubsribeShardAsync(
                    IDatabaseAsync db,
                    IConsumerPlan plan,
                    Func<Announcement, IAck, ValueTask> func,
                    IEventSourceConsumerOptions options,
                    CancellationToken cancellationToken)
        {
            string key = $"{plan.Partition}:{plan.Shard}";
            bool isFirstBatchOrFailure = true;

            CommandFlags flags = CommandFlags.None;

            #region await db.CreateConsumerGroupIfNotExistsAsync(...)

            await db.CreateConsumerGroupIfNotExistsAsync(
                key,
                plan.ConsumerGroup,
                plan.Logger ?? _logger);

            #endregion // await db.CreateConsumerGroupIfNotExistsAsync(...)

            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(
                                        cancellationToken, plan.Cancellation).Token;
            TimeSpan delay = TimeSpan.Zero;
            int emptyBatchCount = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                await HandleBatchAsync();
            }

            #region HandleBatchAsync

            // Handle single batch
            async ValueTask HandleBatchAsync()
            {
                StreamEntry[] results = await ReadBatchAsync();
                emptyBatchCount = results.Length == 0 ? emptyBatchCount + 1 : 0;
                results = await ClaimStaleMessages(emptyBatchCount, results);

                await DelayIfEmpty(results.Length);
                if (results.Length == 0)
                    return;

                try
                {
                    var batchCancellation = new CancellationTokenSource();
                    for (int i = 0; i < results.Length && !batchCancellation.IsCancellationRequested; i++)
                    {
                        StreamEntry result = results[i];

                        #region var announcement = new Announcement(...)

                        Dictionary<RedisValue, RedisValue> entries = result.Values.ToDictionary(m => m.Name, m => m.Value);
                        string channelType = entries[nameof(MetadataExtensions.Empty.ChannelType)];

                        if (channelType != CHANNEL_TYPE)
                        {
                            // TODO: [bnaya 2021-07] send metrics
                            _logger.LogWarning($"{nameof(RedisConsumerChannel)} [{CHANNEL_TYPE}] omit handling message of type '{channelType}'");
                            await AckAsync(result.Id);
                            continue;
                        }

                        string id = entries[nameof(MetadataExtensions.Empty.MessageId)];
                        string operation = entries[nameof(MetadataExtensions.Empty.Operation)];
                        long producedAtUnix = (long)entries[nameof(MetadataExtensions.Empty.ProducedAt)];
                        DateTimeOffset producedAt = DateTimeOffset.FromUnixTimeSeconds(producedAtUnix);
                        var meta = new Metadata
                        {
                            MessageId = id,
                            Partition = plan.Partition,
                            Shard = plan.Shard,
                            Operation = operation,
                            ProducedAt = producedAt
                        };

                        Bucket segmets = await GetBucketAsync(EventBucketCategories.Segments);
                        Bucket interceptions = await GetBucketAsync(EventBucketCategories.Interceptions);

                        #region ValueTask<Bucket> GetBucketAsync(StorageType storageType) // local function

                        async ValueTask<Bucket> GetBucketAsync(EventBucketCategories storageType)
                        {
                            var strategies = plan.StorageStrategy.Where(m => m.IsOfTargetType(storageType));
                            Bucket bucket = Bucket.Empty;
                            if (strategies.Any())
                            {
                                foreach (var strategy in strategies)
                                {
                                    bucket = await strategy.LoadBucketAsync(id, bucket, storageType, meta, LocalGetProperty);
                                }
                            }
                            else
                            {
                                bucket  = await _defaultStorageStrategy.LoadBucketAsync(id, bucket, storageType, meta, LocalGetProperty);
                            }

                            return bucket;

                            string LocalGetProperty (string k) => (string)entries[k];
                        }

                        #endregion // ValueTask<Bucket> StoreBucketAsync(StorageType storageType) // local function

                        var announcement = new Announcement
                        {
                            Metadata = meta,
                            Segments = segmets,
                            InterceptorsData = interceptions
                        };

                        #endregion // var announcement = new Announcement(...)

                        int local = i;
                        var cancellableIds = results[local..].Select(m => m.Id);
                        var ack = new AckOnce(
                                        () => AckAsync(result.Id),
                                        plan.Options.AckBehavior, _logger,
                                        async () =>
                                        {
                                            batchCancellation.CancelSafe(); // cancel forward
                                            await CancelAsync(cancellableIds);
                                        });
                        await func(announcement, ack);
                    }
                }
                catch
                {
                    isFirstBatchOrFailure = true;
                }
            }

            #endregion // HandleBatchAsync

            #region ReadBatchAsync

            // read batch entities from REDIS
            async Task<StreamEntry[]> ReadBatchAsync()
            {
                // TBD: circuit-breaker
                try
                {
                    var r = await _setting.Policy.BatchReading.ExecuteAsync(async () =>
                    {
                        StreamEntry[] values = Array.Empty<StreamEntry>();
                        values = await ReadSelfPending();

                        if (values.Length == 0)
                        {
                            isFirstBatchOrFailure = false;
                            values = await db.StreamReadGroupAsync(
                                                                key,
                                                                plan.ConsumerGroup,
                                                                plan.ConsumerName,
                                                                position: StreamPosition.NewMessages,
                                                                count: options.BatchSize,
                                                                flags: flags);
                        }
                        StreamEntry[] results = values ?? Array.Empty<StreamEntry>();
                        return results;
                    });
                    return r;
                }
                catch (RedisTimeoutException ex)
                {
                    _logger.LogWarning(ex, "Event source [{source}] by [{consumer}]: Timeout", key, plan.ConsumerName);
                    return Array.Empty<StreamEntry>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fail to read from event source [{source}] by [{consumer}]", key, plan.ConsumerName);
                    return Array.Empty<StreamEntry>();
                }
            }

            #endregion // ReadBatchAsync

            #region ReadSelfPending

            // Check for pending messages of the current consumer (crash scenario)
            async Task<StreamEntry[]> ReadSelfPending()
            {
                StreamEntry[] values = Array.Empty<StreamEntry>();
                if (!isFirstBatchOrFailure)
                    return values;

                var pendMsgInfo = await db.StreamPendingMessagesAsync(
                                            key,
                                            plan.ConsumerGroup,
                                            options.BatchSize,
                                            plan.ConsumerName,
                                            flags: CommandFlags.DemandMaster);
                if (pendMsgInfo != null && pendMsgInfo.Length != 0)
                {
                    var ids = pendMsgInfo.Select(m => m.MessageId).ToArray();
                    values = await db.StreamClaimAsync(key,
                                              plan.ConsumerGroup,
                                              plan.ConsumerName,
                                              0,
                                              ids,
                                              flags: CommandFlags.DemandMaster);
                    values = values ?? Array.Empty<StreamEntry>();
                }

                return values;
            }

            #endregion // ReadSelfPending

            #region ClaimStaleMessages

            // Taking work from other consumers which have log-time's pending messages
            async Task<StreamEntry[]> ClaimStaleMessages(
                int emptyBatchCount,
                StreamEntry[] values)
            {
                if (values.Length != 0) return values;
                if (emptyBatchCount < _setting.ClaimingTrigger.EmptyBatchCount)
                    return values;

                StreamPendingInfo pendingInfo = await db.StreamPendingAsync(key, plan.ConsumerGroup, flags: CommandFlags.DemandMaster);
                foreach (var c in pendingInfo.Consumers)
                {
                    var self = c.Name == plan.ConsumerName;
                    if (self) continue;

                    plan.Logger?.LogInformation($"\t\t{c.Name}{self}: {c.PendingMessageCount}");
                    var pendMsgInfo = await db.StreamPendingMessagesAsync(
                        key,
                        plan.ConsumerGroup,
                        10,
                        c.Name, 
                        pendingInfo.LowestPendingMessageId, 
                        pendingInfo.HighestPendingMessageId,
                        flags: CommandFlags.DemandMaster);

                    RedisValue[] ids = pendMsgInfo.Select(m => m.MessageId).ToArray();
                    values = await db.StreamClaimAsync(key,
                                              plan.ConsumerGroup,
                                              c.Name,
                                              (int)_setting.ClaimingTrigger.MinIdleTime.TotalMilliseconds,
                                              ids,
                                              flags: CommandFlags.DemandMaster);
                    if (values != null && values.Length != 0)
                        return values;
                }

                return Array.Empty<StreamEntry>();
            }

            #endregion // ClaimStaleMessages

            #region AckAsync

            // Acknowledge event handling (prevent re-consuming of the message).
            async ValueTask AckAsync(RedisValue messageId)
            {
                try
                {
                    // release the event (won't handle again in the future)
                    long id = await db.StreamAcknowledgeAsync(key,
                                                    plan.ConsumerGroup,
                                                    messageId,
                                                    flags: CommandFlags.DemandMaster);
                }
                catch (Exception)
                { // TODO: [bnaya 2020-10] do better handling (re-throw / swallow + reason) currently logged at the wrapping class
                    throw;
                }
            }

            #endregion // AckAsync

            #region CancelAsync

            // Cancels the asynchronous.
            ValueTask CancelAsync(IEnumerable<RedisValue> messageIds)
            {
                // no way to release consumed item back to the stream
                //try
                //{
                //    // release the event (won't handle again in the future)
                //    await db.StreamClaimIdsOnlyAsync(key,
                //                                    plan.ConsumerGroup,
                //                                    RedisValue.Null,
                //                                    0,
                //                                    messageIds.ToArray(),
                //                                    flags: CommandFlags.DemandMaster);
                //}
                //catch (Exception)
                //{ // TODO: [bnaya 2020-10] do better handling (re-throw / swallow + reason) currently logged at the wrapping class
                //    throw;
                //}
                return ValueTaskStatic.CompletedValueTask;

            }

            #endregion // CancelAsync

            #region DelayIfEmpty

            // avoiding system hit when empty (mitigation of self DDoS)
            async Task<TimeSpan> DelayIfEmpty(int resultsLength)
            {
                if (resultsLength == 0)
                {
                    var cfg = _setting.DelayWhenEmptyBehavior;
                    var newDelay = cfg.CalcNextDelay(delay);
                    var maxDelayMs = Min(cfg.MaxDelay.TotalMilliseconds, delay.TotalMilliseconds);
                    newDelay = TimeSpan.FromMilliseconds(maxDelayMs);
                    await Task.Delay(newDelay, cancellationToken);
                    return newDelay;
                }
                return  TimeSpan.Zero;
            }

            #endregion // DelayIfEmpty
        }

        #endregion // SubsribeShardAsync
    }
}
