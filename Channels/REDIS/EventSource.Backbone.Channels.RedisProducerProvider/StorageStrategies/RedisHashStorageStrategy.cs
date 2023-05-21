using System.Collections.Immutable;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace EventSource.Backbone.Channels
{
    /// <summary>
    /// Responsible to save information to REDIS hash storage.
    /// The information can be either Segmentation or Interception.
    /// When adding it via the builder it can be arrange in a chain in order of having
    /// 'Chain of Responsibility' for saving different parts into different storage (For example GDPR's PII).
    /// Alternative, chain can serve as a cache layer.
    /// </summary>
    internal class RedisHashStorageStrategy : IProducerStorageStrategy
    {
        private readonly IEventSourceRedisConnectionFacroty _connFactory;
        private readonly ILogger _logger;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="connFactory">The connection factory.</param>
        /// <param name="logger">The logger.</param>
        public RedisHashStorageStrategy(
                        IEventSourceRedisConnectionFacroty connFactory,
                        ILogger logger)
        {
            _connFactory = connFactory;
            _logger = logger;
        }

        #endregion // Ctor

        /// <summary>
        /// Saves the bucket information.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="bucket">Either Segments or Interceptions.</param>
        /// <param name="type">The type.</param>
        /// <param name="meta">The metadata.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>
        /// Array of metadata entries which can be used by the consumer side storage strategy, in order to fetch the data.
        /// </returns>
        async ValueTask<IImmutableDictionary<string, string>> IProducerStorageStrategy.SaveBucketAsync(
                                                                    string id,
                                                                    Bucket bucket,
                                                                    EventBucketCategories type,
                                                                    Metadata meta,
                                                                    CancellationToken cancellation)
        {
            var conn = await _connFactory.GetAsync();
            try
            {
                IDatabaseAsync db = conn.GetDatabase();

                var segmentsEntities = bucket
                                                .Select(sgm =>
                                                        new HashEntry(sgm.Key, sgm.Value))
                                                .ToArray();
                var key = $"{meta.FullUri()}:{type}:{id}";
                await db.HashSetAsync(key, segmentsEntities);

                return ImmutableDictionary<string, string>.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fail to Save event's [{id}] buckets [{type}], into the [{URI}] stream: {operation}, IsConnecting: {connecting}",
                    id, type, meta.Uri, meta.Operation, conn.IsConnecting);
                throw;
            }
        }

    }
}
