using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using Weknow.EventSource.Backbone.Channels.RedisProvider;

namespace Weknow.EventSource.Backbone.Channels
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
        private readonly Task<IDatabaseAsync> _dbTask;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="dbTask">The database task.</param>
        public RedisHashStorageStrategy(Task<IDatabaseAsync> dbTask)
        {
            _dbTask = dbTask;
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
            string key = $"{type}~{meta.MessageId}";

            IDatabaseAsync db = await _dbTask;
            var entities = await db.HashGetAllAsync(key, CommandFlags.DemandMaster); // DemandMaster avoid racing
            var pairs = entities.Select(m => ((string)m.Name, (byte[])m.Value));
            var results = prevBucket.TryAddRange(pairs);
            return results;
        }

    }
}
