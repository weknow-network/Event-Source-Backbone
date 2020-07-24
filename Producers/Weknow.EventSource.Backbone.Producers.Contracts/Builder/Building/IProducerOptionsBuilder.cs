
using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
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
        IProducerPartitionBuilder WithOptions(EventSourceOptions options);
    }
}
