
using Microsoft.Extensions.Logging;

using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
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
        /// <param name="options">The options.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        public static IConsumerStoreStrategyBuilder AddS3Strategy(
            this IConsumerStoreStrategyBuilder builder,
            S3Options options = default,
            EventBucketCategories targetType = EventBucketCategories.All)
        {
            var result = builder.AddStorageStrategyFactory(Local, targetType);

            ValueTask<IConsumerStorageStrategy> Local(ILogger logger)
            {
                var factory = S3RepositoryFactory.Create(logger);
                var repo = factory.Get(options);
                var strategy = new S3ConsumerStorageStrategy(repo);
                return strategy.ToValueTask<IConsumerStorageStrategy>();
            }
            return result;
        }


    }
}
