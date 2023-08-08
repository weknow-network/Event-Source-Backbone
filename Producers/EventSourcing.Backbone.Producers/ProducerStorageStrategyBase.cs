using System.Collections.Immutable;

using Microsoft.Extensions.Logging;

namespace EventSourcing.Backbone.Channels
{
    /// <summary>
    /// Responsible of saving information to a storage.
    /// The information can be either Segmentation or Interception.
    /// </summary>
    public abstract class ProducerStorageStrategyBase : IProducerStorageStrategy
    {
        protected readonly ILogger _logger;
        protected readonly TimeSpan? _timeToLive;
        protected readonly Func<Metadata, string, bool>? _filter;
        protected readonly EventBucketCategories _category;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="behavior">
        /// Define the storage behavior
        /// Useful when having multi storage configuration.
        /// May use to implement storage splitting (separation of concerns) like in the case of GDPR.
        /// </param>
        protected ProducerStorageStrategyBase(
                        ILogger logger,
                        StorageBehavior? behavior = null)
        {
            _logger = logger;
            behavior = behavior ?? StorageBehavior.Empty;
            _filter = behavior.Filter;
            _timeToLive = behavior.timeToLive;
            _category = behavior.Category;
        }

        #endregion // Ctor

        #region Name

        /// <summary>
        /// Gets the name of the storage provider.
        /// </summary>
        public abstract string Name { get; }

        #endregion // Name

        #region IProducerStorageStrategy.SaveBucketAsync

        /// <summary>
        /// Saves the bucket information.
        /// </summary>
        /// <param name="bucket">Either Segments or Interceptions.</param>
        /// <param name="type">The type.</param>
        /// <param name="meta">The metadata.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>
        /// Array of metadata entries which can be used by the consumer side storage strategy, in order to fetch the data.
        /// </returns>
        async ValueTask<IImmutableDictionary<string, string>> IProducerStorageStrategy.SaveBucketAsync(
                                                                    Bucket bucket,
                                                                    EventBucketCategories type,
                                                                    Metadata meta,
                                                                    CancellationToken cancellation)
        {
            if ((_category & type) == EventBucketCategories.None)
                return ImmutableDictionary<string, string>.Empty;
            try
            {
                IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>> query = bucket;
                if (_filter != null)
                    query = query.Where(kv => _filter(meta, kv.Key));

                var result = await OnSaveBucketAsync(query, type, meta, cancellation);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fail to Save event's [{id}] buckets [{type}], into the [{URI}], stream: {signature}",
                    meta.MessageId, type, meta.Uri, meta.Signature);
                throw;
            }
        }

        #endregion // IProducerStorageStrategy.SaveBucketAsync

        #region OnSaveBucketAsync

        /// <summary>
        /// Saves the bucket information.
        /// </summary>
        /// <param name="bucket">Either Segments or Interceptions (after filtering).</param>
        /// <param name="type">The type.</param>
        /// <param name="meta">The metadata.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>
        /// Array of metadata entries which can be used by the consumer side storage strategy, in order to fetch the data.
        /// </returns>
        protected abstract ValueTask<IImmutableDictionary<string, string>> OnSaveBucketAsync(
                                                            IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>> bucket,
                                                            EventBucketCategories type,
                                                            Metadata meta,
                                                            CancellationToken cancellation);

        #endregion // OnSaveBucketAsync

        #region IsOfCategory

        /// <summary>
        /// Determines whether is belong to a category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns>
        /// <c>true</c> if is of category; otherwise, <c>false</c>.
        /// </returns>
        bool IProducerStorageStrategy.IsOfCategory(EventBucketCategories category) => (_category & category) != EventBucketCategories.None;

        #endregion // IsOfCategory
    }
}
