using Amazon.S3;

using EventSourcing.Backbone.Building;
using EventSourcing.Backbone.Channels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace EventSourcing.Backbone
{
    /// <summary>
    /// Extension methods for S3 storage strategy.
    /// </summary>
    public static class S3ConsumerStorageStrategyExtension
    {
        /// <summary>
        /// Adds the S3 storage strategy.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="options">The options.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="envAccessKey">Either the access key or environment variable hold it (depend on the fromEnvironment parameters).</param>
        /// <param name="envSecretKey">Either the secret or environment variable hold it (depend on the fromEnvironment parameters)</param>
        /// <param name="envRegion">Either the region or environment variable hold it (depend on the fromEnvironment parameters)</param>
        /// <param name="fromEnvironment">if set to <c>true</c>looks for the access key, secret and region in the environment variables.</param>
        /// <returns></returns>
        public static T AddS3Storage<T>(
            this T builder,
            S3ConsumerOptions? options = null,
            EventBucketCategories targetType = EventBucketCategories.All,
            string envAccessKey = "S3_EVENT_SOURCE_ACCESS_KEY",
            string envSecretKey = "S3_EVENT_SOURCE_SECRET",
            string envRegion = "S3_EVENT_SOURCE_REGION",
            bool fromEnvironment = true)
                    where T : IConsumerStoreStrategyBuilder<T>
        {
            var result = builder.AddStorageStrategyFactory(Local, targetType);

            IConsumerStorageStrategy Local(ILogger logger)
            {
                var factory = S3RepositoryFactory.Create(logger, envAccessKey, envSecretKey, envRegion, fromEnvironment);
                var opt = options ?? S3ConsumerOptions.Default;
                var repo = factory.Get(opt);
                S3ConsumerStorageTuning tune = new S3ConsumerStorageTuning { OverrideKeyIfExists = opt.OverrideKeyIfExists, KeysFilter = opt.KeysFilter };
                var strategy = new S3ConsumerStorageStrategy(repo, tune);
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
        /// <param name="options">The options.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        public static T AddS3Storage<T>(
            this T builder,
            IAmazonS3 client,
            S3ConsumerOptions? options = null,
            EventBucketCategories targetType = EventBucketCategories.All)
                    where T : IConsumerStoreStrategyBuilder<T>
        {
            var result = builder.AddStorageStrategyFactory(Local, targetType);

            IConsumerStorageStrategy Local(ILogger logger)
            {
                var factory = S3RepositoryFactory.Create(logger, client);
                var opt = options ?? S3ConsumerOptions.Default;
                var repo = factory.Get(opt);
                S3ConsumerStorageTuning tune = new S3ConsumerStorageTuning { OverrideKeyIfExists = opt.OverrideKeyIfExists, KeysFilter = opt.KeysFilter };
                var strategy = new S3ConsumerStorageStrategy(repo, tune);
                return strategy;
            }
            return result;
        }

        /// <summary>
        /// Adds the S3 storage strategy.
        /// Will resolve IAmazonS3
        /// See the following article for more details on how can you register Amazon credentials:
        /// https://codewithmukesh.com/blog/aws-credentials-for-dotnet-applications/
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="options">The options.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        public static IConsumerIocStoreStrategyBuilder ResolveS3Storage(
            this IConsumerIocStoreStrategyBuilder builder,
            S3ConsumerOptions? options = null,
            EventBucketCategories targetType = EventBucketCategories.All)
        {
            ILogger? logger = builder.ServiceProvider.GetService<ILogger<S3Options>>();
            IAmazonS3? s3Client = builder.ServiceProvider.GetService<IAmazonS3>();
            if (s3Client != null)
            {
                var injectionResult = builder.AddS3Storage(s3Client, options, targetType);
                logger?.LogInformation("Consumer, Resolving AWS S3 via IAmazonS3 injection (might be via profile)");
                return injectionResult;
            }
            logger?.LogInformation("Consumer, Resolving AWS S3 via environment variable");
            var envVarResult = builder.AddS3Storage(options, targetType);
            return envVarResult;
        }
    }
}
