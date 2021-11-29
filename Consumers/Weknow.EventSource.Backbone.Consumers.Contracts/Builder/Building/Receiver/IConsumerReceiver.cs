using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Receive data (on demand data query).
    /// </summary>
    public interface IConsumerReceiver: 
        IConsumerEnvironmentOfBuilder<IConsumerReceiver>,
        IConsumerPartitionBuilder<IConsumerReceiver>,
        IConsumerReceiverCommands
    {
    }
}
