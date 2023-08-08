using System.Collections.Immutable;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Responsible to save information to storage.
    /// The information can be either Segmentation or Interception.
    /// When adding it via the builder it can be arrange in a chain in order of having
    /// 'Chain of Responsibility' for saving different parts into different storage (For example GDPR's PII).
    /// Alternative, chain can serve as a cache layer.
    /// </summary>
    public interface IProducerStorageStrategy
    {
        /// <summary>
        /// Gets the name of the storage provider.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Determines whether is belong to a category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns>
        ///   <c>true</c> if is of category; otherwise, <c>false</c>.
        /// </returns>
        bool IsOfCategory(EventBucketCategories category);

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
        ValueTask<IImmutableDictionary<string, string>> SaveBucketAsync(
            Bucket bucket,
            EventBucketCategories type,
            Metadata meta,
            CancellationToken cancellation = default);
    }

}
