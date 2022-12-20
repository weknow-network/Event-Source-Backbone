using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Receive data (on demand data query).
    /// </summary>
    public interface IConsumerReceiver :
        IConsumerEnvironmentOfBuilder<IConsumerReceiver>,
        IConsumerPartitionBuilder<IConsumerReceiver>,
        IConsumerReceiverCommands
    {
    }
}
