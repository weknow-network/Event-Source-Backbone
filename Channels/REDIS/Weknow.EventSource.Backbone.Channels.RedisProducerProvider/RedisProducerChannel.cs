using Microsoft.Extensions.Logging;

using Polly;

using StackExchange.Redis;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

using static Weknow.EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;
using OpenTelemetry;
using System.Diagnostics;
using OpenTelemetry.Context.Propagation;

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    internal class RedisProducerChannel : IProducerChannelProvider
    {
        private static readonly ActivitySource ACTIVITY_SOURCE = new ActivitySource(EventSourceConstants.REDIS_PRODUCER_CHANNEL_SOURCE);
        private readonly ILogger _logger;
        private readonly AsyncPolicy _resiliencePolicy;
        private readonly Task<IDatabaseAsync> _dbTask;
        private readonly IProducerStorageStrategy _defaultStorageStrategy;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="redis">The redis database promise.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="resiliencePolicy">The resilience policy for retry.</param>
        public RedisProducerChannel(
                        Task<IConnectionMultiplexer> redis,
                        ILogger logger,
                        AsyncPolicy? resiliencePolicy) : this(GetDB(redis), logger, resiliencePolicy)
        {
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="redis">The redis database promise.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="resiliencePolicy">The resilience policy for retry.</param>
        public RedisProducerChannel(
                        Task<IDatabaseAsync> redis,
                        ILogger logger,
                        AsyncPolicy? resiliencePolicy)
        {
            _logger = logger;
            _resiliencePolicy = resiliencePolicy ??
                                Policy.Handle<Exception>()
                                      .RetryAsync(3);
            _dbTask = redis;
            _defaultStorageStrategy = new RedisHashStorageStrategy(redis, logger);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="redis">The redis database.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="resiliencePolicy">The resilience policy for retry.</param>
        public RedisProducerChannel(
                        IDatabaseAsync redis,
                        ILogger logger,
                        AsyncPolicy? resiliencePolicy) :
                this(Task.FromResult(redis), logger, resiliencePolicy)
        {
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="resiliencePolicy">The resilience policy for retry.</param>
        /// <param name="endpointEnvKey">The endpoint env key.</param>
        /// <param name="passwordEnvKey">The password env key.</param>
        public RedisProducerChannel(
                        ILogger logger,
                        Action<ConfigurationOptions>? configuration,
                        AsyncPolicy? resiliencePolicy,
                        string endpointEnvKey,
                        string passwordEnvKey) :
                this(RedisClientFactory.CreateAsync(
                                            logger,
                                            configuration,
                                            endpointEnvKey,
                                            passwordEnvKey),
                     logger,
                     resiliencePolicy)
        {
        }

        #endregion // Ctor

        #region GetDB

        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <param name="redis">The redis.</param>
        /// <returns></returns>
        private static async Task<IDatabaseAsync> GetDB(Task<IConnectionMultiplexer> redis)
        {
            var mp = await redis;
            return mp.GetDatabase();
        }

        #endregion // GetDB

        #region SendAsync

        /// <summary>
        /// Sends raw announcement.
        /// </summary>
        /// <param name="payload">The raw announcement data.</param>
        /// <param name="storageStrategy">The storage strategy.</param>
        /// <returns>
        /// Return the message id
        /// </returns>
        public async ValueTask<string> SendAsync(
            Announcement payload,
            ImmutableArray<IProducerStorageStrategyWithFilter> storageStrategy)
        {
            Metadata meta = payload.Metadata;
            string id = meta.MessageId;

            #region var entries = new NameValueEntry[]{...}

            // local method 
            NameValueEntry KV(RedisValue key, RedisValue value) => new NameValueEntry(key, value);
            ImmutableArray<NameValueEntry> commonEntries = ImmutableArray.Create(
                KV(nameof(meta.MessageId), id),
                KV(nameof(meta.Operation), meta.Operation),
                KV(nameof(meta.ProducedAt), meta.ProducedAt.ToUnixTimeSeconds()),
                KV(nameof(meta.ChannelType), CHANNEL_TYPE)
            );

            #endregion // var entries = new NameValueEntry[]{...}

            RedisValue messageId = await _resiliencePolicy.ExecuteAsync(LocalStreamAddAsync);

            return messageId;

            #region LocalStreamAddAsync

            async Task<RedisValue> LocalStreamAddAsync()
            {
                await LocalStoreBucketAsync(EventBucketCategories.Segments);
                await LocalStoreBucketAsync(EventBucketCategories.Interceptions);

                var telemetryBuilder = commonEntries.ToBuilder();
                var activityName = $"{meta.Operation} produce";
                using Activity? activity = ACTIVITY_SOURCE.StartActivity(activityName, ActivityKind.Producer);
                activity.InjectSpan(meta, telemetryBuilder, LocalInjectTelemetry);
                meta.InjectTelemetryTags(activity);
                var entries = telemetryBuilder.ToArray();

                try
                {
                    IDatabaseAsync db = await _dbTask;
                    using var scope = SuppressInstrumentationScope.Begin();
                    var result = await db.StreamAddAsync(meta.Key(), entries,
                                                   flags: CommandFlags.DemandMaster);
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fail to push event [{id}] into the [{partition}->{shard}] stream: {operation}",
                        meta.MessageId, meta.Partition, meta.Shard, meta.Operation);
                    throw;
                }

                #region ValueTask StoreBucketAsync(StorageType storageType) // local function

                async ValueTask LocalStoreBucketAsync(EventBucketCategories storageType)
                {
                    var strategies = storageStrategy.Where(m => m.IsOfTargetType(storageType));
                    Bucket bucket = storageType == EventBucketCategories.Segments ? payload.Segments : payload.InterceptorsData;
                    if (strategies.Any())
                    {
                        foreach (var strategy in strategies)
                        {
                            var metaItems = await strategy.SaveBucketAsync(id, bucket, storageType, meta);
                            foreach (var item in metaItems)
                            {
                                commonEntries = commonEntries.Add(KV(item.Key, item.Value));
                            }
                        }
                    }
                    else
                    {
                        await _defaultStorageStrategy.SaveBucketAsync(id, bucket, storageType, meta);
                    }
                }

                #endregion // ValueTask StoreBucketAsync(StorageType storageType) // local function

                #region LocalInjectTelemetry

                void LocalInjectTelemetry(
                                ImmutableArray<NameValueEntry>.Builder builder,
                                string key,
                                string value)
                {
                    builder.Add(KV(key, value));
                }

                #endregion // LocalInjectTelemetry
            }

            #endregion // LocalStreamAddAsync
        }

        #endregion // SendAsync
    }
}
