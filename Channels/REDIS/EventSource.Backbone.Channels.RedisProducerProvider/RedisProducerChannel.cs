using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using OpenTelemetry;

using Polly;

using StackExchange.Redis;

using static EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;

namespace EventSource.Backbone.Channels.RedisProvider
{
    internal class RedisProducerChannel : IProducerChannelProvider
    {
        private static readonly ActivitySource ACTIVITY_SOURCE = new ActivitySource(EventSourceConstants.REDIS_PRODUCER_CHANNEL_SOURCE);
        private readonly ILogger _logger;
        private readonly AsyncPolicy _resiliencePolicy;
        private readonly IEventSourceRedisConnectionFacroty _connFactory;
        private readonly IProducerStorageStrategy _defaultStorageStrategy;
        private const string META_SLOT = "__<META>__";

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="redisFactory">The redis database promise.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="resiliencePolicy">The resilience policy for retry.</param>
        public RedisProducerChannel(
                        IEventSourceRedisConnectionFacroty redisFactory,
                        ILogger logger,
                        AsyncPolicy? resiliencePolicy)
        {
            _connFactory = redisFactory;
            _logger = logger;
            _resiliencePolicy = resiliencePolicy ??
                                Policy.Handle<Exception>()
                                      .RetryAsync(3);
            _defaultStorageStrategy = new RedisHashStorageStrategy(_connFactory, logger);
        }


        #endregion // Ctor

        #region GetDB

        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <param name="redisConnFactory">The redis connection factory.</param>
        /// <returns></returns>
        private static async Task<IDatabaseAsync> GetDB(IEventSourceRedisConnectionFacroty redisConnFactory)
        {
            var mp = await redisConnFactory.GetAsync();
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

            string metaJson = JsonSerializer.Serialize(meta, EventSourceOptions.FullSerializerOptions);

            // local method 
            NameValueEntry KV(RedisValue key, RedisValue value) => new NameValueEntry(key, value);
            ImmutableArray<NameValueEntry> commonEntries = ImmutableArray.Create(
                KV(nameof(meta.MessageId), id),
                KV(nameof(meta.Operation), meta.Operation),
                KV(nameof(meta.ProducedAt), meta.ProducedAt.ToUnixTimeSeconds()),
                KV(nameof(meta.ChannelType), CHANNEL_TYPE),
                KV(nameof(meta.Origin), meta.Origin.ToString()),
                KV(META_SLOT, metaJson)
            );

            #endregion // var entries = new NameValueEntry[]{...}

            RedisValue messageId = await _resiliencePolicy.ExecuteAsync(LocalStreamAddAsync);

            return (string?)messageId ?? "0000000000000-0";

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
                    IConnectionMultiplexer conn = await _connFactory.GetAsync();
                    IDatabaseAsync db = conn.GetDatabase();
                    using var scope = SuppressInstrumentationScope.Begin();
                    var k = meta.FullUri();
                    var result = await db.StreamAddAsync(k, entries,
                                                   flags: CommandFlags.DemandMaster);
                    return result;
                }
                #region Exception Handling

                catch (RedisConnectionException ex)
                {
                    _logger.LogError(ex, "REDIS Connection Failure: push event [{id}] into the [{URI}] stream: {operation}",
                        meta.MessageId, meta.Uri, meta.Operation);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fail to push event [{id}] into the [{URI}] stream: {operation}",
                        meta.MessageId, meta.Uri, meta.Operation);
                    throw;
                }

                #endregion // Exception Handling

                #region ValueTask StoreBucketAsync(StorageType storageType) // local function

                async ValueTask LocalStoreBucketAsync(EventBucketCategories storageType)
                {
                    var strategies = storageStrategy.Where(m => m.IsOfTargetType(storageType));
                    Bucket bucket = storageType == EventBucketCategories.Segments ? payload.Segments : payload.InterceptorsData;
                    if (strategies.Any())
                    {
                        foreach (var strategy in strategies)
                        {
                            await SaveBucketAsync(strategy);
                        }
                    }
                    else
                    {
                        await SaveBucketAsync(_defaultStorageStrategy);
                    }

                    async ValueTask SaveBucketAsync(IProducerStorageStrategy strategy)
                    {
                        IImmutableDictionary<string, string> metaItems =
                            await strategy.SaveBucketAsync(id, bucket, storageType, meta);
                        foreach (var item in metaItems)
                        {
                            commonEntries = commonEntries.Add(KV(item.Key, item.Value));
                        }

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
