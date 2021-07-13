using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Amazon;
using Amazon.S3;

using Microsoft.Extensions.Logging;


namespace Weknow.EventSource.Backbone.Channels
{
    /// <summary>
    /// Abstract S3 operations
    /// </summary>
    public sealed class S3RepositoryFactory : IS3RepositoryFactory
    {
        private readonly ILogger _logger;
        private readonly AmazonS3Client _client;
        private static readonly ImmutableHashSet<string> VALID_HEADERS = ImmutableHashSet<string>.Empty
                                        .Add("Content-Type");
        private readonly ConcurrentDictionary<(string bucket, string? basePath), S3Repository> _cache = new ConcurrentDictionary<(string bucket, string? basePath), S3Repository>();

        #region Create

        /// <summary>
        /// Creates the specified logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        public static IS3RepositoryFactory Create(ILogger logger) => new S3RepositoryFactory(logger);

        #endregion // Create

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public S3RepositoryFactory(
            ILogger<S3Repository> logger) : this((ILogger)logger)
        {
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public S3RepositoryFactory(
            ILogger logger)
        {
            _logger = logger;

            string accessKey = Environment.GetEnvironmentVariable("S3_ACCESS_KEY") ?? "";
            string secretKey = Environment.GetEnvironmentVariable("S3_SECRET") ?? "";
            string? regionKey = Environment.GetEnvironmentVariable("S3_REGION");
            RegionEndpoint rgnKey = (!string.IsNullOrEmpty(regionKey))
                                        ? RegionEndpoint.GetBySystemName(regionKey)
                                        : RegionEndpoint.USEast2;

            _client = new AmazonS3Client(
                                accessKey,
                                secretKey,
                                rgnKey);
        }

        #endregion // Ctor

        #region Get

        /// <summary>
        /// Get repository instance.
        /// </summary>
        /// <param name="bucket">The bucket.</param>
        /// <param name="basePath">The base path.</param>
        /// <returns></returns>
        public S3Repository Get(
            string bucket,
            string? basePath = null)
        {

            return _cache.GetOrAdd((bucket, basePath), CreateInternal);
        }

        #endregion // Get

        #region CreateInternal

        /// <summary>
        /// Creates repository.
        /// </summary>
        /// <param name="props">The props.</param>
        /// <returns></returns>
        private S3Repository CreateInternal(
            (string bucket, string? basePath) props)
        {
            return new S3Repository(_client, props.bucket, _logger, props.basePath);
        }

        #endregion // CreateInternal

        #region Dispose Pattern

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _client.Dispose();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="S3RepositoryFactory"/> class.
        /// </summary>
        ~S3RepositoryFactory()
        {
            Dispose();
        }

        #endregion // Dispose Pattern
    }

}
