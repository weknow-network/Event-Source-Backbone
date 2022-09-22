
using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Channels;


namespace Weknow.EventSource.Backbone
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
        /// <param name="envAccessKey">The environment variable of access key.</param>
        /// <param name="envSecretKey">The environment variable of secret key.</param>
        /// <param name="envRegion">The environment variable of region.</param>
        /// <returns></returns>
        public static IProducerStoreStrategyBuilder AddS3Strategy(
            this IProducerStoreStrategyBuilder builder,
            S3Options options = default,
            EventBucketCategories targetType = EventBucketCategories.All,
            Predicate<string>? filter = null,
            string envAccessKey = "S3_EVENT_SOURCE_ACCESS_KEY",
            string envSecretKey = "S3_EVENT_SOURCE_SECRET",
            string envRegion = "S3_EVENT_SOURCE_REGION")
        {
            var result = builder.AddStorageStrategy(Local, targetType, filter);

            ValueTask<IProducerStorageStrategy> Local(ILogger logger)
            {
                var factory = S3RepositoryFactory.Create(logger, envAccessKey, envSecretKey, envRegion);
                var repo = factory.Get(options);
                var strategy = new S3ProducerStorageStrategy(repo);
                return strategy.ToValueTask<IProducerStorageStrategy>();
            }

            return result;
        }


    }
}
