namespace EventSourcing.Backbone.Building
{
    /// <summary>
    /// Enable configuration.
    /// </summary>
    public interface IProducerOptionsBuilder
        : IProducerEnvironmentBuilder
    {
        /// <summary>
        /// Apply configuration.
        /// </summary>
        IProducerEnvironmentBuilder WithOptions(EventSourceOptions options);

        /// <summary>
        /// Apply configuration.
        /// </summary>
        IProducerEnvironmentBuilder WithOptions(Func<EventSourceOptions, EventSourceOptions> options);
    }
}
