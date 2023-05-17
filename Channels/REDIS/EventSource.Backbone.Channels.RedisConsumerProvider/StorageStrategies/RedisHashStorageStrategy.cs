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
    internal class RedisHashStorageStrategy : IConsumerStorageStrategy
    {
        private readonly IEventSourceRedisConnectionFacroty _connFactory;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="connFactory">The database task.</param>
        public RedisHashStorageStrategy(IEventSourceRedisConnectionFacroty connFactory)
        {
            _connFactory = connFactory;
        }

        /// <summary>
        /// Load the bucket information.
        /// </summary>
        /// <param name="meta">The meta fetch provider.</param>
        /// <param name="prevBucket">The current bucket (previous item in the chain).</param>
        /// <param name="type">The type of the storage.</param>
        /// <param name="getProperty">The get property.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>
        /// Either Segments or Interceptions.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        async ValueTask<Bucket> IConsumerStorageStrategy.LoadBucketAsync(
            Metadata meta,
            Bucket prevBucket,
            EventBucketCategories type,
            Func<string, string> getProperty,
            CancellationToken cancellation)
        {
            string key = $"{meta.Key()}:{type}:{meta.MessageId}";

            IConnectionMultiplexer conn = await _connFactory.GetAsync();
            IDatabaseAsync db = conn.GetDatabase();
            HashEntry[] entities;
            try
            {
                entities = await db.HashGetAllAsync(key, CommandFlags.DemandMaster); // DemandMaster avoid racing
            }
            catch (Exception)
            {
                // backward compatibility
                key = $"{type}~{meta.MessageId}";
                entities = await db.HashGetAllAsync(key, CommandFlags.DemandMaster); // DemandMaster avoid racing
            }

#pragma warning disable CS8600 
#pragma warning disable CS8620
            var pairs = entities.Select(m => ((string)m.Name, (byte[])m.Value));
            var results = prevBucket.TryAddRange(pairs);
#pragma warning restore CS8620
#pragma warning restore CS8600

            return results;

        }

    }
}
