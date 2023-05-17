using EventSource.Backbone.Building;

namespace EventSource.Backbone
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
