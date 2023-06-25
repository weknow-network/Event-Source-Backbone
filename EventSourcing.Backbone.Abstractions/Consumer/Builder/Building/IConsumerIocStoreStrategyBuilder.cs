﻿namespace EventSourcing.Backbone.Building
{
    /// <summary>
    /// storage configuration with IoC.
    /// </summary>
    /// <seealso cref="EventSourcing.Backbone.Building.IConsumerStoreStrategyBuilder" />
    public interface IConsumerIocStoreStrategyBuilder : IConsumerStoreStrategyBuilder
    {
        /// <summary>
        /// Gets the service provider.
        /// </summary>
        IServiceProvider ServiceProvider { get; }
    }
}
