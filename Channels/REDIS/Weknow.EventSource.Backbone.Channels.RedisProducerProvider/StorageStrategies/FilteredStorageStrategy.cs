using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.Channels
{
    /// <summary>
    /// Wrap Channel Storage with key filtering of the bucket.
    /// Useful for 'Chain of Responsibility' by saving different parts
    /// into different storage (For example GDPR's PII).
    /// </summary>
    internal class FilteredStorageStrategy: IProducerStorageStrategyWithFilter
    {
        private readonly IProducerStorageStrategy _storage;
        private readonly Predicate<string>? _filter;
        private readonly StorageType _targetType;
        private readonly static (string key, string metadata)[] EMPTY_RESULT = Array.Empty<(string key, string metadata)>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="storage">The actual storage provider.</param>
        /// <param name="filter">The filter according to keys.</param>
        /// <param name="targetType">Type of the target.</param>
        public FilteredStorageStrategy(
            IProducerStorageStrategy storage,
            Predicate<string>? filter,
            StorageType targetType)
        {
            _storage = storage;
            _filter = filter;
            _targetType = targetType;
        }

        /// <summary>
        /// Determines whether is of the right target type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        bool IProducerStorageStrategyWithFilter.IsOfTargetType(StorageType type) => (_targetType & type) == type;

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
            if ((_targetType & type) != type) return EMPTY_RESULT;
            var filtered =  bucket.RemoveRange(_filter);
            await _storage.SaveBucketAsync(id, filtered, type);
            return EMPTY_RESULT;
        }
    }
}
