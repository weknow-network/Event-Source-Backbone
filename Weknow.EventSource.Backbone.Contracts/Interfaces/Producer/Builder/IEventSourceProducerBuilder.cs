using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IEventSourceProducerBuilder
        : IEventSourceProducerCustomBuilder
    {
        /// <summary>
        /// Gets the main event source.
        /// </summary>
        IEventSourceProducerCustomBuilder WithOptions(EventSourceOptions options);
    }
}
