
using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Enable configuration.
    /// </summary>
    public interface IEventSourceProducerOptionsBuilder
        : IEventSourceProducerPartitionBuilder
    {
        /// <summary>
        /// Attach configuration.
        /// </summary>
        IEventSourceProducerPartitionBuilder WithOptions(EventSourceOptions options);
    }
}
