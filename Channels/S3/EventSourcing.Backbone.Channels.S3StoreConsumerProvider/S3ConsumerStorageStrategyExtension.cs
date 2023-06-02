
using Amazon.Runtime;
using Amazon.S3;

using EventSourcing.Backbone.Building;
using EventSourcing.Backbone.Channels;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;


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
        public static IConsumerStoreStrategyBuilder AddS3Strategy(
            this IConsumerStoreStrategyBuilder builder,
            S3Options options = default,
            EventBucketCategories targetType = EventBucketCategories.All,
            string envAccessKey = "S3_EVENT_SOURCE_ACCESS_KEY",
            string envSecretKey = "S3_EVENT_SOURCE_SECRET",
            string envRegion = "S3_EVENT_SOURCE_REGION",
            bool fromEnvironment = true)
        {
            var result = builder.AddStorageStrategyFactory(Local, targetType);

            ValueTask<IConsumerStorageStrategy> Local(ILogger logger)
            {
                var factory = S3RepositoryFactory.Create(logger, envAccessKey, envSecretKey, envRegion, fromEnvironment);
                var repo = factory.Get(options);
                var strategy = new S3ConsumerStorageStrategy(repo);
                return strategy.ToValueTask<IConsumerStorageStrategy>();
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
        public static IConsumerStoreStrategyBuilder AddS3Strategy(
            this IConsumerStoreStrategyBuilder builder,
            IAmazonS3 client,
            S3Options options = default,
            EventBucketCategories targetType = EventBucketCategories.All)
        {
            var result = builder.AddStorageStrategyFactory(Local, targetType);

            ValueTask<IConsumerStorageStrategy> Local(ILogger logger)
            {
                var factory = S3RepositoryFactory.Create(logger, client);
                var repo = factory.Get(options);
                var strategy = new S3ConsumerStorageStrategy(repo);
                return strategy.ToValueTask<IConsumerStorageStrategy>();
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
        public static IConsumerStoreStrategyBuilder ResolveS3Strategy(
            this IConsumerIocStoreStrategyBuilder builder,
            S3Options options = default,
            EventBucketCategories targetType = EventBucketCategories.All)
        {
            IAmazonS3 s3Client = builder.ServiceProvider.GetService<IAmazonS3>() ?? throw new EventSourcingException("IAmazonS3 is not registered");
            var result = builder.AddS3Strategy(s3Client, options, targetType);
            return result;
        }
    }
}
