using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
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
        /// Load the bucket information.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="prevBucket">The current bucket (previous item in the chain).</param>
        /// <param name="type">The type of the storage.</param>
        /// <param name="meta">The meta fetch provider.</param>
        /// <returns>
        /// Either Segments or Interceptions.
        /// </returns>
        ValueTask<Bucket> LoadBucketAsync(string id, Bucket prevBucket, StorageType type, Func<string, string> meta);
    }
}
