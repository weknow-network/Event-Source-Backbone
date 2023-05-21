﻿using System.Collections.Immutable;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using EventSource.Backbone.Channels;

using static EventSource.Backbone.EventSourceConstants;


namespace EventSource.Backbone
{
    /// <summary>
    /// Responsible to save information to S3 storage.
    /// The information can be either Segmentation or Interception.
    /// When adding it via the builder it can be arrange in a chain in order of having
    /// 'Chain of Responsibility' for saving different parts into different storage (For example GDPR's PII).
    /// Alternative, chain can serve as a cache layer.
    /// </summary>
    public class S3ProducerStorageStrategy : IProducerStorageStrategy
    {
        private readonly IS3Repository _repository;
        private int _index = 0;

        #region ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="s3Repository">S3 repository.
        /// Use S3Factory in order to create it (will create one if missing).
        /// S3Factory will read credentials from the following environment variables: "S3_ACCESS_KEY", "S3_SECRET", "S3_REGION".</param>
        public S3ProducerStorageStrategy(
            IS3Repository s3Repository)
        {
            _repository = s3Repository;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The options.</param>
        /// <param name="factory">The repository's factory.</param>
        public S3ProducerStorageStrategy(
            ILogger logger,
            S3Options options = default,
            IS3RepositoryFactory? factory = null)
        {
            IS3RepositoryFactory facroey = factory ?? S3RepositoryFactory.Create(logger);
            _repository = facroey.Get(options);
        }

        #endregion // ctor

        /// <summary>
        /// Saves the bucket information.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="bucket">Either Segments or Interceptions.</param>
        /// <param name="type">The type.</param>
        /// <param name="meta">The meta.</param>
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
            var date = DateTime.UtcNow;
            int index = Interlocked.Increment(ref _index);
            string basePath = $"{meta.Uri}/{date:yyyy-MM-dd/HH:mm}/{meta.Operation}/{id}/{index}/{type}";
            var tasks = bucket.Select(SaveAsync);
            var propKeyToS3Key = await Task.WhenAll(tasks);
            string json = JsonSerializer.Serialize(propKeyToS3Key, SerializerOptionsWithIndent);
            var result = ImmutableDictionary<string, string>.Empty.Add($"{Constants.PROVIDER_ID}~{type}", json);
            return result;

            async Task<KeyValuePair<string, string>> SaveAsync(KeyValuePair<string, ReadOnlyMemory<byte>> pair)
            {
                var (key, data) = pair;
                string path = $"{basePath}/{key}";
                var response = await _repository.SaveAsync(data, meta.Environment, path, cancellation: cancellation);
                return new KeyValuePair<string, string>(key, path);
            }
        }
    }
}
