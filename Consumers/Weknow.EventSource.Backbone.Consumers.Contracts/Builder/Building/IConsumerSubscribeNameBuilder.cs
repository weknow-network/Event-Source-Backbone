namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Set the consumer's name.
    /// Optional Name of the consumer.
    /// Can use for observability.
    /// </summary>
    public interface IConsumerSubscribeNameBuilder : IConsumerSubscribeBuilder
    {
        /// <summary>
        /// Set the consumer's name
        /// </summary>
        IConsumerSubscribeBuilder Name(string consumerName);
    }
}
