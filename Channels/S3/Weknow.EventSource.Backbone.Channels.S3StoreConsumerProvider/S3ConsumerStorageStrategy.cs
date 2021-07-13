using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace Weknow.EventSource.Backbone.Channels
{
    /// <summary>
    /// Responsible to load information from S3 storage.
    /// The information can be either Segmentation or Interception.
    /// When adding it via the builder it can be arrange in a chain in order of having
    /// 'Chain of Responsibility' for saving different parts into different storage (For example GDPR's PII).
    /// Alternative, chain can serve as a cache layer.
    /// </summary>
    internal class S3ConsumerStorageStrategy : IConsumerStorageStrategy
    {
        private readonly IS3Repository _repository;

        #region ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="s3Repository">S3 repository.
        /// Use S3Factory in order to create it (will create one if missing).
        /// S3Factory will read credentials from the following environment variables: "S3_ACCESS_KEY", "S3_SECRET", "S3_REGION".</param>
        public S3ConsumerStorageStrategy(
            IS3Repository s3Repository)
        {
            _repository = s3Repository;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="bucket">The bucket.</param>
        /// <param name="basePath">The base path.</param>
        /// <param name="factory">The repository's factory.</param>
        public S3ConsumerStorageStrategy(
            ILogger logger,
            string bucket,
            string? basePath = null,
            IS3RepositoryFactory? factory = null)
        {
            IS3RepositoryFactory facroey = factory ?? S3RepositoryFactory.Create(logger);
            _repository = facroey.Get(bucket, basePath);
        }

        #endregion // ctor

        /// <summary>
        /// Load the bucket information.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="prevBucket">The current bucket (previous item in the chain).</param>
        /// <param name="type">The type of the storage.</param>
        /// <param name="meta">The meta fetch provider.</param>
        /// <param name="getProperty">The get property.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>
        /// Either Segments or Interceptions.
        /// </returns>
        async ValueTask<Bucket> IConsumerStorageStrategy.LoadBucketAsync(
            string id, 
            Bucket prevBucket, 
            EventBucketCategories type,
            Metadata meta,
            Func<string, string> getProperty,
            CancellationToken cancellation)
        {
            var lookup = ImmutableDictionary.CreateRange(prevBucket);
            string idsOfItemsStr = getProperty(Constants.Keys.Ids);
            string[] idsOfItems = idsOfItemsStr.Split(Constants.Keys.IdsSeparator);

            var tasks = idsOfItems.Select(LocalFetchAsync);
            var pairs = await Task.WhenAll(tasks);

            Bucket result = prevBucket.TryAddRange(pairs);
            return result;

            async Task<(string key, byte[] value)?> LocalFetchAsync(string itemId)
            {
                if (lookup.ContainsKey(itemId)) return null;
                string path = getProperty(itemId);
                if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(itemId);

                var response = await _repository.GetBytesAsync(path, cancellation);
                return (itemId, response);
            }
        }
    }
}
