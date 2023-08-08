
using Microsoft.Extensions.Logging;


namespace EventSourcing.Backbone.Building
{
    /// <summary>
    /// storage configuration.
    /// </summary>
    public interface IConsumerStoreStrategyBuilder : IConsumerStoreStrategyBuilder<IConsumerStoreStrategyBuilder>
    {
    }

    /// <summary>
    /// storage configuration.
    /// </summary>
    public interface IConsumerStoreStrategyBuilder<T> : IConsumerOptionsBuilder
        where T : IConsumerStoreStrategyBuilder<T>
    {
        /// <summary>
        /// Adds the storage strategy (Segment / Interceptions).
        /// Will use default storage (REDIS Hash) when empty.
        /// When adding more than one it will to all, act as a fall-back (first win, can use for caching).
        /// It important the consumer's storage will be in sync with this setting.
        /// </summary>
        /// <param name="storageStrategy">Storage strategy provider.</param>
        /// <param name="category">Type of the target category.</param>
        /// <returns></returns>
        T AddStorageStrategyFactory(
            Func<ILogger, IConsumerStorageStrategy> storageStrategy,
            EventBucketCategories category = EventBucketCategories.All);
    }
}
