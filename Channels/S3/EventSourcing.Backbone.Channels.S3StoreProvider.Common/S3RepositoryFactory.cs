using System.Collections.Concurrent;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;

using Microsoft.Extensions.Logging;


namespace EventSourcing.Backbone.Channels
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
        /// <param name="accessKey">The access key or environment variable which hold it.</param>
        /// <param name="secret">The secret or environment variable which hold it .</param>
        /// <param name="region">The region environment variable which hold it.</param>
        /// <param name="fromEnvironment">if set to <c>true</c> [will try to find the value from environment variable.</param>
        /// <returns></returns>
        public static IS3RepositoryFactory Create(ILogger logger,
            string accessKey = "S3_EVENT_SOURCE_ACCESS_KEY",
            string secret = "S3_EVENT_SOURCE_SECRET",
            string region = "S3_EVENT_SOURCE_REGION",
            bool fromEnvironment = true)
        {
            accessKey =
                fromEnvironment 
                ? Environment.GetEnvironmentVariable(accessKey) ?? accessKey
                : accessKey;
            secret =
                fromEnvironment
                ? Environment.GetEnvironmentVariable(secret) ?? secret
                : secret;
            string? regionKey =
                fromEnvironment
                ? Environment.GetEnvironmentVariable(region) ?? region
                : region;

            RegionEndpoint rgnKey = (!string.IsNullOrEmpty(regionKey))
                                        ? RegionEndpoint.GetBySystemName(regionKey)
                                        : RegionEndpoint.USEast2;
            var client = new AmazonS3Client(accessKey, secret, rgnKey);
            return new S3RepositoryFactory(logger, client);
        }

        /// <summary>
        /// Creates the specified logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="credentials">The credentials.</param>
        /// <returns></returns>
        public static IS3RepositoryFactory Create(ILogger logger,
            AWSCredentials credentials)
        {
            var client = new AmazonS3Client(credentials);
            return new S3RepositoryFactory(logger, client);
        }

        /// <summary>
        /// Creates the specified logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="credentials">The credentials.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        public static IS3RepositoryFactory Create(ILogger logger,
            AWSCredentials credentials,
            AmazonS3Config configuration)
        {
            var client = new AmazonS3Client(credentials, configuration);
            return new S3RepositoryFactory(logger, client);
        }

        /// <summary>
        /// Creates the specified logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        public static IS3RepositoryFactory Create(ILogger logger,
            AmazonS3Config configuration)
        {
            var client = new AmazonS3Client(configuration);
            return new S3RepositoryFactory(logger, client);
        }

        #endregion // Create

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="client">The client.</param>
        public S3RepositoryFactory(
            ILogger logger,
            AmazonS3Client client)
        {
            _logger = logger;

            _client = client;
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
