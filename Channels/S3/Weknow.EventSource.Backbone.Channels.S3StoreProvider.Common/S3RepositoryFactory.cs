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
        private readonly ConcurrentDictionary<S3Options, S3Repository> _cache = new ConcurrentDictionary<S3Options, S3Repository>();

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
        /// <param name="options">The options.</param>
        /// <returns></returns>
        S3Repository IS3RepositoryFactory.Get(S3Options options)
        {

            var repo = _cache.GetOrAdd(options, CreateInternal);
            repo.AddReference();
            return repo;
        }

        #endregion // Get

        #region CreateInternal

        /// <summary>
        /// Creates repository.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        private S3Repository CreateInternal(
            S3Options options = default)
        {
            return new S3Repository(_client, _logger, options);
        }

        #endregion // CreateInternal
    }

}
