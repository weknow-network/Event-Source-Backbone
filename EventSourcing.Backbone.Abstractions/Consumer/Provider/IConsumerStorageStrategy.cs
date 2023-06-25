﻿namespace EventSourcing.Backbone
{
    /// <summary>
    /// Responsible to load information from storage.
    /// The information can be either Segmentation or Interception.
    /// When adding it via the builder it can be arrange in a chain in order of having
    /// 'Chain of Responsibility' for saving different parts into different storage (For example GDPR's PII).
    /// Alternative, chain can serve as a cache layer.
    /// </summary>
    public interface IConsumerStorageStrategy
    {
        /// <summary>
        /// Gets the name of the storage provider.
        /// </summary>
        string Name { get; }

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
        ValueTask<Bucket> LoadBucketAsync(
            Metadata meta,
            Bucket prevBucket,
            EventBucketCategories type,
            Func<string, string> getProperty,
            CancellationToken cancellation = default);
    }
}
