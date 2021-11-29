using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Wrap Channel Storage with key filtering of the bucket.
    /// Useful for 'Chain of Responsibility' by saving different parts
    /// into different storage (For example GDPR's PII).
    /// </summary>
    internal class FilteredStorageStrategy : IConsumerStorageStrategyWithFilter
    {
        private readonly IConsumerStorageStrategy _storage;
        private readonly EventBucketCategories _targetType;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="storage">The actual storage provider.</param>
        /// <param name="targetType">Type of the target.</param>
        public FilteredStorageStrategy(
            IConsumerStorageStrategy storage,
            EventBucketCategories targetType)
        {
            _storage = storage;
            _targetType = targetType;
        }

        /// <summary>
        /// Determines whether is of the right target type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        bool IConsumerStorageStrategyWithFilter.IsOfTargetType(EventBucketCategories type) => (_targetType & type) == type;

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
        async ValueTask<Bucket> IConsumerStorageStrategy.LoadBucketAsync(
                                                Metadata meta,
                                                Bucket prevBucket,
                                                EventBucketCategories type,
                                                Func<string, string> getProperty,
                                                CancellationToken cancellation)
        {
            if ((_targetType & type) != type) return prevBucket;
            var result = await _storage.LoadBucketAsync(meta, prevBucket, type, getProperty, cancellation);
            return result;
        }
    }
}
