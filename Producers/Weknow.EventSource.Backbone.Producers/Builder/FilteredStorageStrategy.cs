using System.Collections.Immutable;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Wrap Channel Storage with key filtering of the bucket.
    /// Useful for 'Chain of Responsibility' by saving different parts
    /// into different storage (For example GDPR's PII).
    /// </summary>
    internal class FilteredStorageStrategy : IProducerStorageStrategyWithFilter
    {
        private readonly IProducerStorageStrategy _storage;
        private readonly Predicate<string>? _filter;
        private readonly EventBucketCategories _targetType;
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
            EventBucketCategories targetType)
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
        bool IProducerStorageStrategyWithFilter.IsOfTargetType(EventBucketCategories type) => (_targetType & type) == type;

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
            if ((_targetType & type) != type) return ImmutableDictionary<string, string>.Empty;
            var filtered = bucket.RemoveRange(_filter);
            var results = await _storage.SaveBucketAsync(id, filtered, type, meta, cancellation);
            return results;
        }
    }
}
