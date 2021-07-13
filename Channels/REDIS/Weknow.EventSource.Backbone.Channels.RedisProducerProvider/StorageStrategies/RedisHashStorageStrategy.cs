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
    internal class RedisHashStorageStrategy :  IProducerStorageStrategy
    {
        private readonly Task<IDatabaseAsync> _dbTask;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="dbTask">The database task.</param>
        public RedisHashStorageStrategy(
                        Task<IDatabaseAsync> dbTask)
        {
            _dbTask = dbTask;
        }

        /// <summary>
        /// Saves the bucket information.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="bucket">Either Segments or Interceptions.</param>
        /// <param name="type">The type.</param>
        /// <returns>
        /// Array of metadata entries which can be used by the consumer side storage strategy, in order to fetch the data.
        /// </returns>
        async ValueTask<(string key, string metadata)[]> IProducerStorageStrategy.SaveBucketAsync(string id, Bucket bucket, StorageType type)
        {
            IDatabaseAsync db = await _dbTask;

            var segmentsEntities = bucket
                                            .Select(sgm =>
                                                    new HashEntry(sgm.Key, sgm.Value))
                                            .ToArray();
            await db.HashSetAsync($"{type}~{id}", segmentsEntities);
            return Array.Empty<(string key, string metadata)>();
        }
    }
}
