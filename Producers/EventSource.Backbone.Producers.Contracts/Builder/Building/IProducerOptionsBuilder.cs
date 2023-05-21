﻿namespace EventSource.Backbone.Building
{
    /// <summary>
    /// Enable configuration.
    /// </summary>
    public interface IProducerOptionsBuilder
        : IProducerEnvironmentBuilder, IProducerLoggerBuilder<IProducerOptionsBuilder>
    {
        /// <summary>
        /// Apply configuration.
        /// </summary>
        IProducerEnvironmentBuilder WithOptions(EventSourceOptions options);
    }
}
