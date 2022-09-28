using Microsoft.Extensions.Logging;
using StackExchange.Redis;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

using Weknow.EventSource.Backbone.Private;

using static System.Math;

using static Weknow.EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;
using Weknow.Text.Json;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;
using Weknow.EventSource.Backbone.Building;
using System.Collections.Concurrent;
using Polly;
using System.Text;

// TODO: [bnaya 2021-07] MOVE TELEMETRY TO THE BASE CLASSES OF PRODUCER / CONSUME

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    /// <summary>
    /// The redis consumer channel.
    /// </summary>
    internal class RedisConsumerChannel : IConsumerChannelProvider
    {
        private const string BEGIN_OF_STREAM = "0000000000000";
        /// <summary>
        /// Max delay
        /// </summary>
        private const int MAX_DELAY = 5000;
        /// <summary>
        /// The read by identifier chunk size.
        /// REDIS don't have option to read direct position (it read from a position, not includes the position itself),
        /// therefore read should start before the actual position.
        /// </summary>
        private const int READ_BY_ID_CHUNK_SIZE = 10;
        /// <summary>
        /// Receiver max iterations
        /// </summary>
        private const int READ_BY_ID_ITERATIONS = 1000 / READ_BY_ID_CHUNK_SIZE;
        private static readonly ActivitySource ACTIVITY_SOURCE = new ActivitySource(EventSourceConstants.REDIS_CONSUMER_CHANNEL_SOURCE);

        private readonly ILogger _logger;
        private readonly RedisConsumerChannelSetting _setting;
        private readonly IEventSourceRedisConnectionFacroty _connFactory;
        private readonly IConsumerStorageStrategy _defaultStorageStrategy;
        private const string META_SLOT = "__<META>__";

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="redisConnFactory">The redis provider promise.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="setting">The setting.</param>
        public RedisConsumerChannel(
                        IEventSourceRedisConnectionFacroty redisConnFactory,
                        ILogger logger,
                        RedisConsumerChannelSetting? setting = null)
        {
            _logger = logger;
            _connFactory = redisConnFactory;
            _defaultStorageStrategy = new RedisHashStorageStrategy(redisConnFactory);
            _setting = setting ?? RedisConsumerChannelSetting.Default;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="setting">The setting.</param>
        /// <param name="credentialsKeys">Environment keys of the credentials</param>
        public RedisConsumerChannel(
                        ILogger logger,
                        Action<ConfigurationOptions>? configuration = null,
                        RedisConsumerChannelSetting? setting = null,
                        RedisCredentialsKeys credentialsKeys = default) : this(
                            new EventSourceRedisConnectionFacroty(
                                                    logger,
                                                    configuration,
                                                    credentialsKeys),
                            logger,
                            setting)
        {
        }

        #endregion // Ctor

        #region SubsribeAsync

        /// <summary>
        /// Subscribe to the channel for specific metadata.
        /// </summary>
        /// <param name="plan">The consumer plan.</param>
        /// <param name="func">The function.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// When completed
        /// </returns>
        public async ValueTask SubsribeAsync(
                    IConsumerPlan plan,
                    Func<Announcement, IAck, ValueTask> func,
                    CancellationToken cancellationToken)
        {
            var joinCancellation = CancellationTokenSource.CreateLinkedTokenSource(plan.Cancellation, cancellationToken).Token;
            ConsumerOptions options = plan.Options;

            while (!joinCancellation.IsCancellationRequested)
            {
                try
                {
                    if (plan.Shard != string.Empty)
                        await SubsribeShardAsync(plan, func, options, joinCancellation);
                    else
                        await SubsribePartitionAsync(plan, func, options, joinCancellation);

                    if (options.FetchUntilUnixDateOrEmpty != null)
                        break;
                }
                #region Exception Handling

                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fail to subscribe into the [{partition}->{shard}] event stream",
                        plan.Partition, plan.Shard);
                    throw;
                }

                #endregion // Exception Handling
            }
        }

        #endregion // SubsribeAsync

        #region SubsribePartitionAsync

        /// <summary>
        /// Subscribe to all shards under a partition.
        /// </summary>
        /// <param name="plan">The consumer plan.</param>
        /// <param name="func">The function.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// When completed
        /// </returns>
        private async ValueTask SubsribePartitionAsync(
                    IConsumerPlan plan,
                    Func<Announcement, IAck, ValueTask> func,
                    ConsumerOptions options,
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
                    // infinite until cancellation (return unique shareds)
                    var keys = GetKeysUnsafeAsync(pattern: $"{partition}:*")
                                                    .WithCancellation(cancellationToken);

                    await foreach (string key in keys)
                    {
                        string shard = key.Substring(partitionSplit);
                        IConsumerPlan p = plan.WithShard(shard);
                        // infinite task (until cancellation)
                        Task subscription = SubsribeShardAsync(plan, func, options, cancellationToken);
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

            // run until cancellation or error
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
        /// <param name="plan">The consumer plan.</param>
        /// <param name="func">The function.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task SubsribeShardAsync(
                    IConsumerPlan plan,
                    Func<Announcement, IAck, ValueTask> func,
                    ConsumerOptions options,
                    CancellationToken cancellationToken)
        {
            var claimingTrigger = options.ClaimingTrigger;
            var minIdleTime = (int)options.ClaimingTrigger.MinIdleTime.TotalMilliseconds;

            string key = plan.Key(); //  $"{plan.Partition}:{plan.Shard}";
            bool isFirstBatchOrFailure = true;

            CommandFlags flags = CommandFlags.None;
            string? fetchUntil = options.FetchUntilUnixDateOrEmpty?.ToString();

            ILogger logger = plan.Logger;

            #region await db.CreateConsumerGroupIfNotExistsAsync(...)


            await _connFactory.CreateConsumerGroupIfNotExistsAsync(
                key,
                plan.ConsumerGroup,
                logger);

            #endregion // await db.CreateConsumerGroupIfNotExistsAsync(...)

            TimeSpan delay = TimeSpan.Zero;
            int emptyBatchCount = 0;
            while (!cancellationToken.IsCancellationRequested && await HandleBatchAsync())
            {
            }

            #region HandleBatchAsync

            // Handle single batch
            async ValueTask<bool> HandleBatchAsync()
            {
                var policy = _setting.Policy.Policy;
                return await policy.ExecuteAsync(HandleBatchBreakerAsync, cancellationToken);
            }


            async Task<bool> HandleBatchBreakerAsync(CancellationToken ct)
            {
                ct.ThrowIfCancellationRequested();

                StreamEntry[] results = await ReadBatchAsync();
                emptyBatchCount = results.Length == 0 ? emptyBatchCount + 1 : 0;
                results = await ClaimStaleMessages(emptyBatchCount, results, ct);

                if (results.Length == 0)
                {
                    if (fetchUntil == null)
                        delay = await DelayIfEmpty(delay, cancellationToken);
                    return fetchUntil == null;
                }

                ct.ThrowIfCancellationRequested();

                try
                {
                    var batchCancellation = new CancellationTokenSource();
                    for (int i = 0; i < results.Length && !batchCancellation.IsCancellationRequested; i++)
                    {
                        StreamEntry result = results[i];

                        #region Metadata meta = ...

                        Dictionary<RedisValue, RedisValue> channelMeta = result.Values.ToDictionary(m => m.Name, m => m.Value);
                        Metadata meta;
                        string? metaJson = channelMeta[META_SLOT];
                        string eventKey = ((string?)result.Id) ?? throw new ArgumentException(nameof(MetadataExtensions.Empty.EventKey));
                        if (string.IsNullOrEmpty(metaJson))
                        { // backward comparability

                            string channelType = ((string?)channelMeta[nameof(MetadataExtensions.Empty.ChannelType)]) ?? throw new ArgumentNullException(nameof(MetadataExtensions.Empty.ChannelType));

                            if (channelType != CHANNEL_TYPE)
                            {
                                // TODO: [bnaya 2021-07] send metrics
                                logger.LogWarning($"{nameof(RedisConsumerChannel)} [{CHANNEL_TYPE}] omit handling message of type '{channelType}'");
                                await AckAsync(result.Id);
                                continue;
                            }

                            string id = ((string?)channelMeta[nameof(MetadataExtensions.Empty.MessageId)]) ?? throw new ArgumentNullException(nameof(MetadataExtensions.Empty.MessageId));
                            string operation = ((string?)channelMeta[nameof(MetadataExtensions.Empty.Operation)]) ?? throw new ArgumentNullException(nameof(MetadataExtensions.Empty.Operation));
                            long producedAtUnix = (long)channelMeta[nameof(MetadataExtensions.Empty.ProducedAt)];
                            DateTimeOffset producedAt = DateTimeOffset.FromUnixTimeSeconds(producedAtUnix);
                            if (fetchUntil != null && string.Compare(fetchUntil, result.Id) < 0)
                                return false;
                            meta = new Metadata
                            {
                                MessageId = id,
                                EventKey = eventKey,
                                Environment = plan.Environment,
                                Partition = plan.Partition,
                                Shard = plan.Shard,
                                Operation = operation,
                                ProducedAt = producedAt
                            };

                        }
                        else
                        {
                            //byte[] metabytes = Convert.FromBase64String(meta64);
                            //string metaJson = Encoding.UTF8.GetString(metabytes);
                            meta = JsonSerializer.Deserialize<Metadata>(metaJson, EventSourceOptions.FullSerializerOptions) ?? throw new ArgumentNullException(nameof(Metadata)); //, EventSourceJsonContext..Metadata);
                            meta = meta with { EventKey = eventKey };

                        }

                        #endregion // Metadata meta = ...

                        #region OriginFilter

                        if (plan.Options.OriginFilter != MessageOrigin.None && (plan.Options.OriginFilter & meta.Origin) == MessageOrigin.None)
                        {
                            continue;
                        }

                        #endregion // OriginFilter

                        #region var announcement = new Announcement(...)

                        Bucket segmets = await GetBucketAsync(plan, channelMeta, meta, EventBucketCategories.Segments);
                        Bucket interceptions = await GetBucketAsync(plan, channelMeta, meta, EventBucketCategories.Interceptions);

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
                                        plan.Options.AckBehavior, logger,
                                        async () =>
                                        {
                                            batchCancellation.CancelSafe(); // cancel forward
                                            await CancelAsync(cancellableIds);
                                        });

                        #region Start Telemetry Span

                        ActivityContext parentContext = meta.ExtractSpan(channelMeta, ExtractTraceContext);
                        // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
                        // https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#span-name
                        var activityName = $"{meta.Operation} consume";

                        bool traceAsParent = (DateTimeOffset.UtcNow - meta.ProducedAt) < plan.Options.TraceAsParent;
                        ActivityContext parentActivityContext = traceAsParent ? parentContext : default(ActivityContext);
                        using var activity = ACTIVITY_SOURCE.StartActivity(
                                                                activityName,
                                                                ActivityKind.Consumer,
                                                                parentActivityContext, links: new[] { new ActivityLink(parentContext) });
                        meta.InjectTelemetryTags(activity);

                        #region IEnumerable<string> ExtractTraceContext(Dictionary<RedisValue, RedisValue> entries, string key)

                        IEnumerable<string> ExtractTraceContext(Dictionary<RedisValue, RedisValue> entries, string key)
                        {
                            try
                            {
                                if (entries.TryGetValue(key, out var value))
                                {
                                    if (string.IsNullOrEmpty(value))
                                        return Array.Empty<string>();
                                    return new[] { value.ToString() };
                                }
                            }
                            #region Exception Handling

                            catch (Exception ex)
                            {
                                Exception err = ex.FormatLazy();
                                _logger.LogError(err, "Failed to extract trace context: {error}", err);
                            }

                            #endregion // Exception Handling

                            return Enumerable.Empty<string>();
                        }

                        #endregion // IEnumerable<string> ExtractTraceContext(Dictionary<RedisValue, RedisValue> entries, string key)

                        #endregion // Start Telemetry Span

                        await func(announcement, ack);
                    }
                }
                catch
                {
                    isFirstBatchOrFailure = true;
                }
                return true;
            }

            #endregion // HandleBatchAsync

            #region ReadBatchAsync

            // read batch entities from REDIS
            async Task<StreamEntry[]> ReadBatchAsync()
            {
                // TBD: circuit-breaker
                try
                {
                    var r = await _setting.Policy.Policy.ExecuteAsync(async (ct) =>
                    {
                        ct.ThrowIfCancellationRequested();
                        StreamEntry[] values = Array.Empty<StreamEntry>();
                        values = await ReadSelfPending();

                        if (values.Length == 0)
                        {
                            isFirstBatchOrFailure = false;

                            IConnectionMultiplexer conn = await _connFactory.GetAsync();
                            IDatabaseAsync db = conn.GetDatabase();
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
                    }, cancellationToken);
                    return r;
                }
                #region Exception Handling

                catch (RedisTimeoutException ex)
                {
                    logger.LogWarning(ex, "Event source [{source}] by [{consumer}]: Timeout", key, plan.ConsumerName);
                    return Array.Empty<StreamEntry>();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Fail to read from event source [{source}] by [{consumer}]", key, plan.ConsumerName);
                    return Array.Empty<StreamEntry>();
                }

                #endregion // Exception Handling
            }

            #endregion // ReadBatchAsync

            #region ReadSelfPending

            // Check for pending messages of the current consumer (crash scenario)
            async Task<StreamEntry[]> ReadSelfPending()
            {
                StreamEntry[] values = Array.Empty<StreamEntry>();
                if (!isFirstBatchOrFailure)
                    return values;


                IConnectionMultiplexer conn = await _connFactory.GetAsync();
                IDatabaseAsync db = conn.GetDatabase();
                StreamPendingMessageInfo[] pendMsgInfo = await db.StreamPendingMessagesAsync(
                                            key,
                                            plan.ConsumerGroup,
                                            options.BatchSize,
                                            plan.ConsumerName,
                                            flags: CommandFlags.DemandMaster);
                if (pendMsgInfo != null && pendMsgInfo.Length != 0)
                {
                    var ids = pendMsgInfo
                        .Select(m => m.MessageId).ToArray();
                    if (ids.Length != 0)
                    {
                        values = await db.StreamClaimAsync(key,
                                                  plan.ConsumerGroup,
                                                  plan.ConsumerName,
                                                  0,
                                                  ids,
                                                  flags: CommandFlags.DemandMaster);
                        values = values ?? Array.Empty<StreamEntry>();
                        _logger.LogInformation("Claimed messages: {ids}", ids);
                    }
                }

                return values;
            }

            #endregion // ReadSelfPending

            #region ClaimStaleMessages

            // Taking work from other consumers which have log-time's pending messages
            async Task<StreamEntry[]> ClaimStaleMessages(
                int emptyBatchCount,
                StreamEntry[] values,
                CancellationToken ct)
            {
                ct.ThrowIfCancellationRequested();
                if (values.Length != 0) return values;
                if (emptyBatchCount < claimingTrigger.EmptyBatchCount)
                    return values;
                try
                {
                    IDatabaseAsync db = await _connFactory.GetDatabaseAsync();
                    StreamPendingInfo pendingInfo = await db.StreamPendingAsync(key, plan.ConsumerGroup, flags: CommandFlags.DemandMaster);
                    foreach (var c in pendingInfo.Consumers)
                    {
                        var self = c.Name == plan.ConsumerName;
                        if (self) continue;
                        try
                        {
                            var pendMsgInfo = await db.StreamPendingMessagesAsync(
                                key,
                                plan.ConsumerGroup,
                                10,
                                c.Name,
                                pendingInfo.LowestPendingMessageId,
                                pendingInfo.HighestPendingMessageId,
                                flags: CommandFlags.DemandMaster);



                            RedisValue[] ids = pendMsgInfo
                                        .Where(x => x.IdleTimeInMilliseconds > minIdleTime)
                                        .Select(m => m.MessageId).ToArray();
                            if (ids.Length == 0)
                                continue;

                            plan.Logger.LogInformation("Event Source Consumer [{name}]: Claimed {count} messages, from Consumer [{name}]", plan.ConsumerName, c.PendingMessageCount, c.Name);
                            // will claim messages only if older than _setting.ClaimingTrigger.MinIdleTime
                            values = await db.StreamClaimAsync(key,
                                                      plan.ConsumerGroup,
                                                      c.Name,
                                                      minIdleTime,
                                                      ids,
                                                      flags: CommandFlags.DemandMaster);
                            if (values.Length != 0)
                                plan.Logger.LogInformation("Event Source Consumer [{name}]: Claimed {count} messages, from Consumer [{name}]", plan.ConsumerName, c.PendingMessageCount, c.Name);
                        }
                        #region Exception Handling

                        catch (RedisTimeoutException ex)
                        {
                            plan.Logger.LogWarning(ex, "Timeout (handle pending): {name}{self}", c.Name, self);
                            continue;
                        }

                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Fail to claim pending: {name}{self}", c.Name, self);
                        }

                        #endregion // Exception Handling

                        if (values != null && values.Length != 0)
                            return values;
                    }
                }
                #region Exception Handling

                catch (RedisConnectionException ex)
                {
                    _logger.LogWarning(ex, "Fail to claim REDIS's pending");
                }

                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fail to claim pending");
                }

                #endregion // Exception Handling

                return Array.Empty<StreamEntry>();
            }

            #endregion // ClaimStaleMessages

            #region AckAsync

            // Acknowledge event handling (prevent re-consuming of the message).
            async ValueTask AckAsync(RedisValue messageId)
            {
                try
                {
                    IConnectionMultiplexer conn = await _connFactory.GetAsync();
                    IDatabaseAsync db = conn.GetDatabase();
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
                return ValueTask.CompletedTask;

            }

            #endregion // CancelAsync
        }

        #endregion // SubsribeShardAsync

        #region GetByIdAsync

        /// <summary>
        /// Gets announcement data by id.
        /// </summary>
        /// <param name="entryId">The entry identifier.</param>
        /// <param name="plan">The plan.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">IConsumerChannelProvider.GetAsync of [{entryId}] from [{plan.Partition}->{plan.Shard}] return nothing.</exception>
        /// <exception cref="System.DataMisalignedException">IConsumerChannelProvider.GetAsync of [{entryId}] from [{plan.Partition}->{plan.Shard}] was expecting single result but got [{entries.Length}] results</exception>
        async ValueTask<Announcement> IConsumerChannelProvider.GetByIdAsync(
            EventKey entryId,
            IConsumerPlan plan,
            CancellationToken cancellationToken)
        {
            string mtdName = $"{nameof(IConsumerChannelProvider)}.{nameof(IConsumerChannelProvider.GetByIdAsync)}";

            try
            {
                IConnectionMultiplexer conn = await _connFactory.GetAsync();
                IDatabaseAsync db = conn.GetDatabase();
                ILogger logger = plan.Logger;
                StreamEntry entry = await FindAsync(entryId);

                #region var announcement = new Announcement(...)

                Dictionary<RedisValue, RedisValue> channelMeta = entry.Values.ToDictionary(m => m.Name, m => m.Value);
                string channelType = GetMeta(nameof(MetadataExtensions.Empty.ChannelType));
                string id = GetMeta(nameof(MetadataExtensions.Empty.MessageId));
                string operation = GetMeta(nameof(MetadataExtensions.Empty.Operation));
                long producedAtUnix = (long)channelMeta[nameof(MetadataExtensions.Empty.ProducedAt)];

                string GetMeta(string propKey)
                {
                    string? result = channelMeta[propKey];
                    if (result == null)  throw new ArgumentNullException(propKey);
                    return result;
                }

                DateTimeOffset producedAt = DateTimeOffset.FromUnixTimeSeconds(producedAtUnix);
#pragma warning disable CS8601 // Possible null reference assignment.
                var meta = new Metadata
                {
                    MessageId = id,
                    EventKey = entry.Id,
                    Environment = plan.Environment,
                    Partition = plan.Partition,
                    Shard = plan.Shard,
                    Operation = operation,
                    ProducedAt = producedAt,
                    ChannelType = channelType
                };
#pragma warning restore CS8601 // Possible null reference assignment.

                Bucket segmets = await GetBucketAsync(plan, channelMeta, meta, EventBucketCategories.Segments);
                Bucket interceptions = await GetBucketAsync(plan, channelMeta, meta, EventBucketCategories.Interceptions);

                var announcement = new Announcement
                {
                    Metadata = meta,
                    Segments = segmets,
                    InterceptorsData = interceptions
                };

                #endregion // var announcement = new Announcement(...)

                return announcement;

                #region FindAsync

                async Task<StreamEntry> FindAsync(EventKey entryId)
                {
                    string lookForId = (string)entryId;
                    string key = plan.Key();

                    string originId = lookForId;
                    int len = originId.IndexOf('-');
                    string fromPrefix = originId.Substring(0, len);
                    long start = long.Parse(fromPrefix);
                    string startPosition = (start - 1).ToString();
                    int iteration = 0;
                    for (int i = 0; i < READ_BY_ID_ITERATIONS; i++) // up to 1000 items
                    {
                        iteration++;
                        StreamEntry[] entries = await db.StreamReadAsync(
                                                                key,
                                                                startPosition,
                                                                READ_BY_ID_CHUNK_SIZE,
                                                                CommandFlags.DemandMaster);
                        if (entries.Length == 0)
                            throw new KeyNotFoundException($"{mtdName} of [{lookForId}] from [{key}] return nothing, start at ({startPosition}, iteration = {iteration}).");
                        string k = string.Empty;
                        foreach (StreamEntry e in entries)
                        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                            k = e.Id;
                            string ePrefix = k.Substring(0, len);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                            long comp = long.Parse(ePrefix);
                            if (comp < start)
                                continue; // not there yet
                            if (k == lookForId)
                            {
                                return e;
                            }
                            if (ePrefix != fromPrefix)
                                throw new KeyNotFoundException($"{mtdName} of [{lookForId}] from [{key}] return not exists.");
                        }
                        startPosition = k; // next batch will start from last entry
                    }
                    throw new KeyNotFoundException($"{mtdName} of [{lookForId}] from [{key}] return not found.");
                }

                #endregion // FindAsync
            }
            #region Exception Handling

            catch (Exception ex)
            {
                string key = plan.Key();
                _logger.LogError(ex.FormatLazy(), "{mtd} Failed: Entry [{entryId}] from [{key}] event stream",
                    mtdName, entryId, key);
                throw;
            }

            #endregion // Exception Handling
        }

        #endregion // GetByIdAsync

        #region GetAsyncEnumerable

        /// <summary>
        /// Gets asynchronous enumerable of announcements.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        async IAsyncEnumerable<Announcement> IConsumerChannelProvider.GetAsyncEnumerable(
                    IConsumerPlan plan,
                    ConsumerAsyncEnumerableOptions? options,
                    [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            string mtdName = $"{nameof(IConsumerChannelProvider)}.{nameof(IConsumerChannelProvider.GetAsyncEnumerable)}";

            IConnectionMultiplexer conn = await _connFactory.GetAsync();
            IDatabaseAsync db = conn.GetDatabase();
            ILogger logger = plan.Logger;
            var loop = AsyncLoop().WithCancellation(cancellationToken);
            await foreach (StreamEntry entry in loop)
            {
                if (cancellationToken.IsCancellationRequested) yield break;

                #region var announcement = new Announcement(...)

                Dictionary<RedisValue, RedisValue> channelMeta = entry.Values.ToDictionary(m => m.Name, m => m.Value);
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8601 // Possible null reference assignment.
                string channelType = channelMeta[nameof(MetadataExtensions.Empty.ChannelType)];
                string id = channelMeta[nameof(MetadataExtensions.Empty.MessageId)];
                string operation = channelMeta[nameof(MetadataExtensions.Empty.Operation)];
                long producedAtUnix = (long)channelMeta[nameof(MetadataExtensions.Empty.ProducedAt)];
                DateTimeOffset producedAt = DateTimeOffset.FromUnixTimeSeconds(producedAtUnix);
                var meta = new Metadata
                {
                    MessageId = id,
                    EventKey = entry.Id,
                    Environment = plan.Environment,
                    Partition = plan.Partition,
                    Shard = plan.Shard,
                    Operation = operation,
                    ProducedAt = producedAt
                };
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                var filter = options?.OperationFilter;
                if (filter != null && !filter(meta))
                    continue;

                Bucket segmets = await GetBucketAsync(plan, channelMeta, meta, EventBucketCategories.Segments);
                Bucket interceptions = await GetBucketAsync(plan, channelMeta, meta, EventBucketCategories.Interceptions);

                var announcement = new Announcement
                {
                    Metadata = meta,
                    Segments = segmets,
                    InterceptorsData = interceptions
                };

                #endregion // var announcement = new Announcement(...)

                yield return announcement;
            };

            #region AsyncLoop

            async IAsyncEnumerable<StreamEntry> AsyncLoop()
            {
                string key = plan.Key();

                int iteration = 0;
                RedisValue startPosition = options?.From ?? BEGIN_OF_STREAM;
                TimeSpan delay = TimeSpan.Zero;
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested) yield break;

                    iteration++;
                    StreamEntry[] entries = await db.StreamReadAsync(
                                                            key,
                                                            startPosition,
                                                            READ_BY_ID_CHUNK_SIZE,
                                                            CommandFlags.DemandMaster);
                    if (entries.Length == 0)
                    {
                        if (options?.ExitWhenEmpty ?? true) yield break;
                        delay = await DelayIfEmpty(delay, cancellationToken);
                    }
                    string k = string.Empty;
                    foreach (StreamEntry e in entries)
                    {
                        if (cancellationToken.IsCancellationRequested) yield break;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                        k = e.Id;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                        if (options?.To != null && string.Compare(options?.To, k) < 0)
                            yield break;
                        yield return e;
                    }
                    startPosition = k; // next batch will start from last entry
                }
            }

            #endregion // AsyncLoop
        }

        #endregion // GetAsyncEnumerable

        #region ValueTask<Bucket> GetBucketAsync(StorageType storageType) // local function

        /// <summary>
        /// Gets a data bucket.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="channelMeta">The channel meta.</param>
        /// <param name="meta">The meta.</param>
        /// <param name="storageType">Type of the storage.</param>
        /// <returns></returns>
        private async ValueTask<Bucket> GetBucketAsync(
                                            IConsumerPlan plan,
                                            Dictionary<RedisValue, RedisValue> channelMeta,
                                            Metadata meta,
                                            EventBucketCategories storageType)
        {

            IEnumerable<IConsumerStorageStrategyWithFilter> strategies = await plan.StorageStrategiesAsync;
            strategies = strategies.Where(m => m.IsOfTargetType(storageType));
            Bucket bucket = Bucket.Empty;
            if (strategies.Any())
            {
                foreach (var strategy in strategies)
                {
                    bucket = await strategy.LoadBucketAsync(meta, bucket, storageType, LocalGetProperty);
                }
            }
            else
            {
                bucket = await _defaultStorageStrategy.LoadBucketAsync(meta, bucket, storageType, LocalGetProperty);
            }

            return bucket;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
            string LocalGetProperty(string k) => (string)channelMeta[k];
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }

        #endregion // ValueTask<Bucket> StoreBucketAsync(StorageType storageType) // local function

        #region DelayIfEmpty

        // avoiding system hit when empty (mitigation of self DDoS)
        private async Task<TimeSpan> DelayIfEmpty(TimeSpan previousDelay, CancellationToken cancellationToken)
        {
            var cfg = _setting.DelayWhenEmptyBehavior;
            var newDelay = cfg.CalcNextDelay(previousDelay);
            var limitDelay = Min(cfg.MaxDelay.TotalMilliseconds, newDelay.TotalMilliseconds);
            newDelay = TimeSpan.FromMilliseconds(limitDelay);
            await Task.Delay(newDelay, cancellationToken);
            return newDelay;
        }

        #endregion // DelayIfEmpty

        #region GetKeysUnsafeAsync

        /// <summary>
        /// Gets the keys unsafe asynchronous.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async IAsyncEnumerable<string> GetKeysUnsafeAsync(
                        string pattern,
                        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            IConnectionMultiplexer multiplexer = await _connFactory.GetAsync();
            var distict = new HashSet<string>();
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (EndPoint endpoint in multiplexer.GetEndPoints())
                {
                    IServer server = multiplexer.GetServer(endpoint);
                    // TODO: [bnaya 2020_09] check the pagination behavior
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
                    await foreach (string key in server.KeysAsync(pattern: pattern))
                    {
                        if (distict.Contains(key))
                            continue;
                        distict.Add(key);
                        yield return key;
                    }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                }
            }
        }

        #endregion // GetKeysUnsafeAsync
    }
}
