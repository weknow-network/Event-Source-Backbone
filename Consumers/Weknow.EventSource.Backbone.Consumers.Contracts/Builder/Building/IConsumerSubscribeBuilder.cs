using System;

namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IConsumerSubscribeBuilder
    {
        /// <summary>
        /// Subscribe consumer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factory">The factory.</param>
        /// <param name="consumerGroup">Consumer Group allow a group of clients to cooperate
        /// consuming a different portion of the same stream of messages</param>
        /// <param name="consumerName">
        /// Optional Name of the consumer.
        /// Can use for observability.
        /// </param>
        /// <returns>
        /// Remove subscription.
        /// keeping the disposable will prevent the consumer to be collected
        /// by th GC (when the behavior don't indicate to hook it until cancellation or dispose).
        /// </returns>
        IAsyncDisposable Subscribe<T>(
            Func<ConsumerMetadata, T> factory,
            string? consumerGroup = null, 
            string? consumerName = null);
    }
}
