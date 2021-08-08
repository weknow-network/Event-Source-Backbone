
using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Enable configuration.
    /// </summary>
    public interface IConsumerStoreStrategyBuilder : IConsumerOptionsBuilder
    {
        /// <summary>
        /// Adds the storage strategy (Segment / Interceptions).
        /// Will use default storage (REDIS Hash) when empty.
        /// When adding more than one it will to all, act as a fall-back (first win, can use for caching).
        /// It important the consumer's storage will be in sync with this setting.
        /// </summary>
        /// <param name="storageStrategy">Storage strategy provider.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        IConsumerStoreStrategyBuilder AddStorageStrategyFactory(
            Func<ILogger, ValueTask<IConsumerStorageStrategy>> storageStrategy,
            EventBucketCategories targetType = EventBucketCategories.All);
    }
}
