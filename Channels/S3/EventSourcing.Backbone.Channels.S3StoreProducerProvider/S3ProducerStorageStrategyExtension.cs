using Amazon.S3;

using EventSourcing.Backbone.Channels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Extension methods for S3 storage strategy.
    /// </summary>
    public static class S3ProducerStorageStrategyExtension
    {
        /// <summary>
        /// Adds the S3 storage strategy.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="filterByOperationAndKey">
        /// Useful when having multi storage configuration.
        /// May use to implement storage splitting (separation of concerns) like in the case of GDPR .
        /// The predicate signature is: (metadata, key) => bool
        ///   the key is driven from the method parameter.
        /// </param>
        /// <param name="options">The options.</param>
        /// <param name="envAccessKey">Either the access key or environment variable hold it (depend on the fromEnvironment parameters).</param>
        /// <param name="envSecretKey">Either the secret or environment variable hold it (depend on the fromEnvironment parameters)</param>
        /// <param name="envRegion">Either the region or environment variable hold it (depend on the fromEnvironment parameters)</param>
        /// <param name="fromEnvironment">if set to <c>true</c>looks for the access key, secret and region in the environment variables.</param>
        /// <returns></returns>
        public static T AddS3Storage<T>(
            this T builder,
            Func<Metadata, string, bool> filterByOperationAndKey,
            S3Options? options = null,
            string envAccessKey = "S3_EVENT_SOURCE_ACCESS_KEY",
            string envSecretKey = "S3_EVENT_SOURCE_SECRET",
            string envRegion = "S3_EVENT_SOURCE_REGION",
            bool fromEnvironment = true)
                    where T : IProducerStoreStrategyBuilder<T>
        {
            return builder.AddS3Storage<T>(options, (StorageBehavior)filterByOperationAndKey, envAccessKey, envSecretKey, envRegion, fromEnvironment);
        }

        /// <summary>
        /// Adds the S3 storage strategy.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="options">The options.</param>
        /// <param name="behavior">
        /// Define the storage behavior
        /// Useful when having multi storage configuration.
        /// May use to implement storage splitting (separation of concerns) like in the case of GDPR.
        /// </param>
        /// <param name="envAccessKey">Either the access key or environment variable hold it (depend on the fromEnvironment parameters).</param>
        /// <param name="envSecretKey">Either the secret or environment variable hold it (depend on the fromEnvironment parameters)</param>
        /// <param name="envRegion">Either the region or environment variable hold it (depend on the fromEnvironment parameters)</param>
        /// <param name="fromEnvironment">if set to <c>true</c>looks for the access key, secret and region in the environment variables.</param>
        /// <returns></returns>
        public static T AddS3Storage<T>(
            this T builder,
            S3Options? options = null,
            StorageBehavior? behavior = null,
            string envAccessKey = "S3_EVENT_SOURCE_ACCESS_KEY",
            string envSecretKey = "S3_EVENT_SOURCE_SECRET",
            string envRegion = "S3_EVENT_SOURCE_REGION",
            bool fromEnvironment = true)
                    where T : IProducerStoreStrategyBuilder<T>
        {
            var result = builder.AddStorageStrategy(Local);

            IProducerStorageStrategy Local(ILogger logger)
            {
                var factory = S3RepositoryFactory.Create(logger, envAccessKey, envSecretKey, envRegion, fromEnvironment);
                var repo = factory.Get(options ?? S3Options.Default);
                var strategy = new S3ProducerStorageStrategy(repo, logger, behavior);
                return strategy;
            }

            return result;
        }

        /// <summary>
        /// Adds the S3 storage strategy.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="client">
        /// S3 client.
        /// Learn how to setup an AWS client: https://codewithmukesh.com/blog/aws-credentials-for-dotnet-applications/
        /// </param>
        /// <param name="filterByOperationAndKey">
        /// Useful when having multi storage configuration.
        /// May use to implement storage splitting (separation of concerns) like in the case of GDPR .
        /// The predicate signature is: (metadata, key) => bool
        ///   the key is driven from the method parameter.
        /// </param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static T AddS3Storage<T>(
            this T builder,
            IAmazonS3 client,
            Func<Metadata, string, bool> filterByOperationAndKey,
            S3Options? options = null)
                where T : IProducerStoreStrategyBuilder<T>
        {
            return builder.AddS3Storage<T>(client, options, filterByOperationAndKey);
        }

        /// <summary>
        /// Adds the S3 storage strategy.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="client">
        /// S3 client.
        /// Learn how to setup an AWS client: https://codewithmukesh.com/blog/aws-credentials-for-dotnet-applications/
        /// </param>
        /// <param name="options">The options.</param>
        /// <param name="behavior">
        /// Define the storage behavior
        /// Useful when having multi storage configuration.
        /// May use to implement storage splitting (separation of concerns) like in the case of GDPR.
        /// </param>
        /// <returns></returns>
        public static T AddS3Storage<T>(
            this T builder,
            IAmazonS3 client,
            S3Options? options = null,
            StorageBehavior? behavior = null)
                where T : IProducerStoreStrategyBuilder<T>
        {
            var result = builder.AddStorageStrategy(Local);

            IProducerStorageStrategy Local(ILogger logger)
            {
                var factory = S3RepositoryFactory.Create(logger, client);
                var repo = factory.Get(options ?? S3Options.Default);
                var strategy = new S3ProducerStorageStrategy(repo, logger, behavior);
                return strategy;
            }

            return result;
        }

        /// <summary>
        /// Adds the S3 storage strategy.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="filterByOperationAndKey">
        /// Useful when having multi storage configuration.
        /// May use to implement storage splitting (separation of concerns) like in the case of GDPR .
        /// The predicate signature is: (metadata, key) => bool
        ///   the key is driven from the method parameter.
        /// </param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static IProducerIocStoreStrategyBuilder ResolveS3Storage(
            this IProducerIocStoreStrategyBuilder builder,
            Func<Metadata, string, bool> filterByOperationAndKey,
            S3Options? options = null)
        {
            return builder.ResolveS3Storage(options, filterByOperationAndKey);
        }

        /// <summary>
        /// Adds the S3 storage strategy.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="options">The options.</param>
        /// <param name="behavior">
        /// Define the storage behavior
        /// Useful when having multi storage configuration.
        /// May use to implement storage splitting (separation of concerns) like in the case of GDPR.
        /// </param>
        /// <returns></returns>
        public static IProducerIocStoreStrategyBuilder ResolveS3Storage(
            this IProducerIocStoreStrategyBuilder builder,
            S3Options? options = null,
            StorageBehavior? behavior = null)
        {
            ILogger? logger = builder.ServiceProvider.GetService<ILogger<S3Options>>();
            IAmazonS3? s3Client = builder.ServiceProvider.GetService<IAmazonS3>();

            if (s3Client != null)
            {
                var injectionResult =
                    builder.AddS3Storage(s3Client, options, behavior);
                logger?.LogInformation("Producer, Resolving AWS S3 via IAmazonS3 injection (might be via profile)");
                return injectionResult;
            }
            logger?.LogInformation("Producer, Resolving AWS S3 via environment variable");

            var envVarResult = builder.AddS3Storage(options, behavior);
            return envVarResult;
        }
    }
}
