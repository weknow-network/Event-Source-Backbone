using EventSourcing.Backbone.Building;

namespace EventSourcing.Backbone
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
