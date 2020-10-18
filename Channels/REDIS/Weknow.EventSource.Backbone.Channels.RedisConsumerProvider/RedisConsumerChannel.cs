using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Private;

using static System.Math;

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    internal class RedisConsumerChannel : IConsumerChannelProvider
    {
        private const int MAX_DELAY = 5000;

        private readonly ILogger _logger;
        private readonly ConfigurationOptions _options;
        private static int _index;
        private const string CONNECTION_NAME_PATTERN = "Event_Source_Consumer_{0}";
        private readonly RedisClientFactory _redisClientFactory;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RedisConsumerChannel(
            ILogger logger,
            ConfigurationOptions options,
            string endpointEnvKey,
            string passwordEnvKey)
        {
            _logger = logger;
            _options = options;
            string name = string.Format(
                                    CONNECTION_NAME_PATTERN,
                                    Interlocked.Increment(ref _index));
            _redisClientFactory = new RedisClientFactory(
                                                logger,
                                                name,
                                                RedisUsageIntent.Read,
                                                endpointEnvKey, passwordEnvKey);
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
            IDatabaseAsync db = await _redisClientFactory.GetDbAsync();
            if (plan.Shard != string.Empty)
                await SubsribeShardAsync(db, plan, func, options, cancellationToken);
            else
                await SubsribePartitionAsync(db, plan, func, options, cancellationToken);
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
            // TODO: if !shard read all keys with partition prefix & subscribe to, interval should check for new shard
            string key = $"{plan.Partition}:{plan.Shard}";
            CommandFlags flags = CommandFlags.None;

            await db.CreateConsumerGroupIfNotExistsAsync(
                key,
                plan.ConsumerGroup,
                plan.Logger ?? _logger);

            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(
                                        cancellationToken, plan.Cancellation).Token;
            int delay = 1;
            while (!cancellationToken.IsCancellationRequested)
            {
                StreamEntry[] results = await ReadBatchAsync();

                await DelayIfEmpty(results.Length);

                foreach (StreamEntry result in results)
                {
                    var entries = result.Values.ToDictionary(m => m.Name, m => m.Value);
                    string id = entries[nameof(Metadata.Empty.MessageId)];
                    string segmentsKey = $"Segments~{id}";
                    string interceptorsKey = $"Interceptors~{id}";

                    string operation = entries[nameof(Metadata.Empty.Operation)];
                    long producedAtUnix = (long)entries[nameof(Metadata.Empty.ProducedAt)];
                    DateTimeOffset producedAt = DateTimeOffset.FromUnixTimeSeconds(producedAtUnix);
                    var meta = new Metadata(id, plan.Partition, plan.Shard, operation, producedAt);

                    var segmentsEntities = await db.HashGetAllAsync(segmentsKey, CommandFlags.DemandMaster); // DemandMaster avoid racing
                    var segmentsPairs = segmentsEntities.Select(m => ((string)m.Name, (byte[])m.Value));
                    var interceptionsEntities = await db.HashGetAllAsync(interceptorsKey, CommandFlags.DemandMaster); // DemandMaster avoid racing
                    var interceptionsPairs = interceptionsEntities.Select(m => ((string)m.Name, (byte[])m.Value));

                    var segmets = Bucket.Empty.AddRange(segmentsPairs);
                    var interceptions = Bucket.Empty.AddRange(interceptionsPairs);

                    var announcement = new Announcement(meta, segmets, interceptions);
                    var ack = new AckOnce(() => AckAsync(result.Id), plan.Options.AckBehavior, _logger);
                    await func(announcement, ack);
                }
            }

            #region ReadBatchAsync

            // read batch entities from REDIS
            async Task<StreamEntry[]> ReadBatchAsync()
            {
                // TBD: circuit-breaker
                try
                {
                    StreamEntry[] values = await db.StreamReadGroupAsync(
                                                        key,
                                                        plan.ConsumerGroup,
                                                        plan.ConsumerName,
                                                        position: StreamPosition.NewMessages,
                                                        count: options.BatchSize,
                                                        flags: flags);
                    return values ?? Array.Empty<StreamEntry>();

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

            #region DelayIfEmpty

            // avoiding system hit when empty (mitigation of self DDoS)
            async Task<int> DelayIfEmpty(int resultsLength)
            {
                if (resultsLength == 0)
                {
                    await Task.Delay(delay, cancellationToken);
                    delay *= Max(delay, 2);
                    delay = Min(MAX_DELAY, delay);
                }
                else
                    delay = 1;
                return delay;
            }

            #endregion // DelayIfEmpty
        }

        #endregion // SubsribeShardAsync
    }
}
