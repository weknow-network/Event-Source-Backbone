
using Microsoft.Extensions.Logging;

using Weknow.EventSource.Backbone.Channels;


namespace Weknow.EventSource.Backbone
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
        /// <param name="logger">The logger.</param>
        /// <param name="bucket">The bucket.</param>
        /// <param name="basePath">The base path.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        public static IConsumerStoreStrategyBuilder AddS3Strategy(
            this IConsumerStoreStrategyBuilder builder,
            ILogger logger,
            string? bucket = null,
            string? basePath = null,
            EventBucketCategories targetType = EventBucketCategories.All)
        {
            var factory = S3RepositoryFactory.Create(logger);
            var repo = factory.Get(bucket, basePath);
            var strategy = new S3ConsumerStorageStrategy(repo);
            var result = builder.AddStorageStrategy(strategy);
            return result;
        }


    }
}
