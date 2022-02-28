namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Set the consumer group
    /// Consumer Group allow a group of clients to cooperate
    /// consuming a different portion of the same stream of messages
    /// </summary>
    public interface IConsumerSubscribeGroupBuilder : IConsumerSubscribeNameBuilder
    {
        /// <summary>
        /// Consumer's group name.
        /// </summary>
        /// <param name="consumerGroup">Name of the group.</param>
        /// <returns></returns>
        IConsumerSubscribeNameBuilder Group(string consumerGroup);
    }   
}
