namespace EventSourcing.Backbone;

/// <summary>
/// storage configuration with IoC.
/// </summary>
/// <seealso cref="EventSourcing.Backbone.IProducerStoreStrategyBuilder" />
public interface IProducerIocStoreStrategyBuilder : IProducerStoreStrategyBuilder<IProducerIocStoreStrategyBuilder>
{
    /// <summary>
    /// Gets the service provider.
    /// </summary>
    IServiceProvider ServiceProvider { get; }
}
