using System;
using System.Threading.Tasks;

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
        /// <param name="instance">The factory.</param>
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
        IConsumerLifetime Subscribe<T>(
            T instance,
            string? consumerGroup = null,
            string? consumerName = null);

        /// <summary>
        /// Subscribe consumer.
        /// </summary>
        /// <param name="handlers">Per operation invocation handler, handle methods calls.</param>
        /// <returns>
        /// Remove subscription.
        /// keeping the disposable will prevent the consumer to be collected
        /// by th GC (when the behavior don't indicate to hook it until cancellation or dispose).
        /// </returns>
        IConsumerLifetime Subscribe(
            params Func<Announcement, IConsumerBridge, Task>[] handlers);

        /// <summary>
        /// Subscribe consumer.
        /// </summary>
        /// <param name="handlers">Per operation invocation handler, handle methods calls.</param>
        /// <param name="consumerGroup">Consumer Group allow a group of clients to cooperate
        /// consuming a different portion of the same stream of messages</param>
        /// <param name="consumerName">Optional Name of the consumer.
        /// Can use for observability.</param>
        /// <returns>
        /// Remove subscription.
        /// keeping the disposable will prevent the consumer to be collected
        /// by th GC (when the behavior don't indicate to hook it until cancellation or dispose).
        /// </returns>
        IConsumerLifetime Subscribe(
            Func<Announcement, IConsumerBridge, Task>[] handlers,
            string? consumerGroup = null,
            string? consumerName = null);
    }
}
