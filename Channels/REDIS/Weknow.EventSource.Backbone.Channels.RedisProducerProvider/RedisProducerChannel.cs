using Microsoft.Extensions.Logging;

using Polly;

using StackExchange.Redis;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static Weknow.EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    internal class RedisProducerChannel : IProducerChannelProvider
    {
        private readonly ILogger _logger;
        private readonly AsyncPolicy _resiliencePolicy;
        private static int _index = 0;
        private const string CONNECTION_NAME_PATTERN = "Event_Source_Producer_{0}";
        private readonly Task<IDatabaseAsync> _dbTask;
        internal ImmutableArray<IProducerStorageStrategyWithFilter> StorageStrategy { get; } = ImmutableArray<IProducerStorageStrategyWithFilter>.Empty;
        private readonly IProducerStorageStrategy _defaultStorageStrategy;

        #region Ctor

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
                        string passwordEnvKey)
        {
            _logger = logger;
            _resiliencePolicy = resiliencePolicy ??
                                Policy.Handle<Exception>()
                                      .RetryAsync(3); // TODO: [bnaya 2021-02] onRetry -> open telemetry
            string name = string.Format(
                                    CONNECTION_NAME_PATTERN,
                                    Interlocked.Increment(ref _index));
            var redisClientFactory = new RedisClientFactory(
                                                logger,
                                                name,
                                                RedisUsageIntent.Write,
                                                configuration,
                                                endpointEnvKey, passwordEnvKey);
            _dbTask = redisClientFactory.GetDbAsync();
            _defaultStorageStrategy = new RedisHashStorageStrategy(_dbTask);

        }
        /// <summary>
        /// Copy ctor.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="storageStrategy">The storage strategy.</param>
        internal RedisProducerChannel(
                        RedisProducerChannel self,
                        ImmutableArray<IProducerStorageStrategyWithFilter> storageStrategy)
        {
            _logger = self._logger;
            _resiliencePolicy = self._resiliencePolicy;
            _dbTask = self._dbTask;
            _defaultStorageStrategy = self._defaultStorageStrategy;
            StorageStrategy = storageStrategy;
        }

        #endregion // Ctor

        #region SendAsync

        /// <summary>
        /// Sends raw announcement.
        /// </summary>
        /// <param name="payload">The raw announcement data.</param>
        /// <returns>
        /// Return the message id
        /// </returns>
        public async ValueTask<string> SendAsync(Announcement payload)
        {
            IDatabaseAsync db = await _dbTask;

            Metadata meta = payload.Metadata;
            string id = meta.MessageId;

            #region var entries = new NameValueEntry[]{...}

            // local method
            NameValueEntry KV(RedisValue key, RedisValue value) => new NameValueEntry(key, value);
            var entriesBuilder = ImmutableArray.Create<NameValueEntry>(
                KV(nameof(meta.MessageId), id),
                KV(nameof(meta.Operation), meta.Operation),
                KV(nameof(meta.ProducedAt), meta.ProducedAt.ToUnixTimeSeconds()),
                KV(nameof(meta.ChannelType), CHANNEL_TYPE)
            );

            #endregion // var entries = new NameValueEntry[]{...}

            await StoreBucketAsync(EventBucketCategories.Segments);
            await StoreBucketAsync(EventBucketCategories.Interceptions);

            #region ValueTask StoreBucketAsync(StorageType storageType) // local function

            async ValueTask StoreBucketAsync(EventBucketCategories storageType)
            {
                var strategies = StorageStrategy.Where(m => m.IsOfTargetType(storageType));
                Bucket bucket = storageType == EventBucketCategories.Segments ? payload.Segments : payload.InterceptorsData;
                if (strategies.Any())
                {
                    foreach (var strategy in strategies)
                    {
                        var metaItems = await strategy.SaveBucketAsync(id, bucket, storageType, meta);
                        foreach (var item in metaItems)
                        {
                            entriesBuilder = entriesBuilder.Add(KV(item.Key, item.Value));
                        }
                    }
                }
                else
                {
                    await _defaultStorageStrategy.SaveBucketAsync(id, payload.Segments, storageType, meta);
                }
            }

            #endregion // ValueTask StoreBucketAsync(StorageType storageType) // local function

            var entries = entriesBuilder.ToArray();
            RedisValue messageId = await _resiliencePolicy.ExecuteAsync(() =>
                                                db.StreamAddAsync(meta.Key(), entries,
                                                   flags: CommandFlags.DemandMaster));


            return messageId;
        }

        #endregion // SendAsync
    }
}
