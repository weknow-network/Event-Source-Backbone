using System;
using System.Collections;
using System.Collections.Immutable;
using System.Threading.Tasks.Dataflow;

namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Enable configuration.
    /// </summary>
    public interface IEventSourceConsumerOptionsBuilder
        : IEventSourceConsumerHooksBuilder
    {
        /// <summary>
        /// Attach configuration.
        /// </summary>
        IEventSourceConsumerHooksBuilder WithOptions(EventSourceConsumerOptions options);
    }
}
