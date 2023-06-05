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
        /// <param name="options">The options.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="filter">The filter of which keys in the bucket will be store into this storage.</param>
        /// <param name="envAccessKey">Either the access key or environment variable hold it (depend on the fromEnvironment parameters).</param>
        /// <param name="envSecretKey">Either the secret or environment variable hold it (depend on the fromEnvironment parameters)</param>
        /// <param name="envRegion">Either the region or environment variable hold it (depend on the fromEnvironment parameters)</param>
        /// <param name="fromEnvironment">if set to <c>true</c>looks for the access key, secret and region in the environment variables.</param>
        /// <returns></returns>
        public static IProducerStoreStrategyBuilder AddS3Storage(
            this IProducerStoreStrategyBuilder builder,
            S3Options options = default,
            EventBucketCategories targetType = EventBucketCategories.All,
            Predicate<string>? filter = null,
            string envAccessKey = "S3_EVENT_SOURCE_ACCESS_KEY",
            string envSecretKey = "S3_EVENT_SOURCE_SECRET",
            string envRegion = "S3_EVENT_SOURCE_REGION",
            bool fromEnvironment = true)
        {
            var result = builder.AddStorageStrategy(Local, targetType, filter);

            ValueTask<IProducerStorageStrategy> Local(ILogger logger)
            {
                var factory = S3RepositoryFactory.Create(logger, envAccessKey, envSecretKey, envRegion, fromEnvironment);
                var repo = factory.Get(options);
                var strategy = new S3ProducerStorageStrategy(repo);
                return strategy.ToValueTask<IProducerStorageStrategy>();
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
        /// <param name="filter">The filter of which keys in the bucket will be store into this storage.</param>
        /// <returns></returns>
        public static IProducerStoreStrategyBuilder AddS3Storage(
            this IProducerStoreStrategyBuilder builder,
            IAmazonS3 client,
            S3Options options = default,
            EventBucketCategories targetType = EventBucketCategories.All,
            Predicate<string>? filter = null)
        {
            var result = builder.AddStorageStrategy(Local, targetType, filter);

            ValueTask<IProducerStorageStrategy> Local(ILogger logger)
            {
                var factory = S3RepositoryFactory.Create(logger, client);
                var repo = factory.Get(options);
                var strategy = new S3ProducerStorageStrategy(repo);
                return strategy.ToValueTask<IProducerStorageStrategy>();
            }

            return result;
        }

        /// <summary>
        /// Adds the S3 storage strategy.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="options">The options.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="filter">The filter of which keys in the bucket will be store into this storage.</param>
        /// <returns></returns>
        public static IProducerStoreStrategyBuilder ResolveS3Storage(
            this IProducerIocStoreStrategyBuilder builder,
            S3Options options = default,
            EventBucketCategories targetType = EventBucketCategories.All,
            Predicate<string>? filter = null)
        {
            IAmazonS3 s3Client = builder.ServiceProvider.GetService<IAmazonS3>() ?? throw new EventSourcingException("IAmazonS3 is not registered");
            IProducerStoreStrategyBuilder result = builder.AddS3Storage(s3Client, options, targetType, filter);
            return result;
        }
    }
}
