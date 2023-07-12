
using EventSourcing.Backbone.Building;

using Microsoft.Extensions.Logging;

using Polly;

namespace EventSourcing.Backbone;

/// <summary>
/// storage configuration.
/// </summary>
public interface IProducerStoreStrategyBuilder<T> : IProducerOptionsBuilder
    where T : IProducerStoreStrategyBuilder<T>
{
    /// <summary>
    /// Adds the storage strategy (Segment / Interceptions).
    /// Will use default storage (REDIS Hash) when empty.
    /// When adding more than one it will to all, act as a fall-back (first win, can use for caching).
    /// It important the consumer's storage will be in sync with this setting.
    /// </summary>
    /// <param name="storageStrategy">Storage strategy provider.</param>
    /// <returns></returns>
    T AddStorageStrategy(
        Func<ILogger, IProducerStorageStrategy> storageStrategy);
}


/// <summary>
/// storage configuration.
/// </summary>
/// <seealso cref="EventSourcing.Backbone.Building.IProducerOptionsBuilder" />
public interface IProducerStoreStrategyBuilder : IProducerStoreStrategyBuilder<IProducerStoreStrategyBuilder>
{
}
