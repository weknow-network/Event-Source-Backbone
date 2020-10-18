namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Enable configuration.
    /// </summary>
    public interface IProducerOptionsBuilder
        : IProducerPartitionBuilder
    {
        /// <summary>
        /// Apply configuration.
        /// </summary>
        IProducerPartitionBuilder WithOptions(IEventSourceOptions options);
    }
}
