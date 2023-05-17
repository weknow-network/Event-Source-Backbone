
using Microsoft.Extensions.Logging;

using EventSource.Backbone.Building;

namespace EventSource.Backbone
{
    /// <summary>
    /// Enable configuration.
    /// </summary>
    /// <seealso cref="EventSource.Backbone.Building.IProducerOptionsBuilder" />
    public interface IProducerStoreStrategyBuilder : IProducerOptionsBuilder
    {
        /// <summary>
        /// Adds the storage strategy (Segment / Interceptions).
        /// Will use default storage (REDIS Hash) when empty.
        /// When adding more than one it will to all, act as a fall-back (first win, can use for caching).
        /// It important the consumer's storage will be in sync with this setting.
        /// </summary>
        /// <param name="storageStrategy">Storage strategy provider.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="filter">The filter of which keys in the bucket will be store into this storage.</param>
        /// <returns></returns>
        IProducerStoreStrategyBuilder AddStorageStrategy(
            Func<ILogger, ValueTask<IProducerStorageStrategy>> storageStrategy,
            EventBucketCategories targetType = EventBucketCategories.All,
            Predicate<string>? filter = null);
    }
}
