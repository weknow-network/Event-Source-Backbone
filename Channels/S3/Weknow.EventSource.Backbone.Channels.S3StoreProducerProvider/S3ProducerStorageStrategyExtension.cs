
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
        /// <param name="bucket">The bucket.</param>
        /// <param name="basePath">The base path.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="filter">The filter of which keys in the bucket will be store into this storage.</param>
        /// <returns></returns>
        public static IProducerStoreStrategyBuilder AddS3Strategy(
            this IProducerStoreStrategyBuilder builder,
            string? bucket = null,
            string? basePath = null,
            EventBucketCategories targetType = EventBucketCategories.All,
            Predicate<string>? filter = null)
        {
            var result = builder.AddStorageStrategy(Local, targetType, filter);

            ValueTask<IProducerStorageStrategy> Local(ILogger logger)
            {
                var factory = S3RepositoryFactory.Create(logger);
                var repo = factory.Get(bucket, basePath);
                var strategy = new S3ProducerStorageStrategy(repo);
                return strategy.ToValueTask<IProducerStorageStrategy>();
            }

            return result;
        }


    }
}
