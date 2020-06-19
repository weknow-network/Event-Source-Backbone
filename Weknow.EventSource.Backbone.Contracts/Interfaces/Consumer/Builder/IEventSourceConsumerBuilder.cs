using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Event Source Consumer builder.
    /// </summary>
    public interface IEventSourceConsumerBuilder: IEventSourceConsumerCustomBuilder
    {
        /// <summary>
        /// Gets the main event source.
        /// </summary>
        IEventSourceConsumerCustomBuilder WithOptions(EventSourceOptions options);
    }
}
