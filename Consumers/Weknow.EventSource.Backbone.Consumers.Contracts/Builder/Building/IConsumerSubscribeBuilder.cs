using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.Building
{

    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IConsumerSubscribeBuilder: 
        IConsumerEnvironmentOfBuilder<IConsumerSubscribeBuilder>,
        IConsumerPartitionBuilder<IConsumerSubscribeBuilder>
        //IConsumerShardOfBuilder<IConsumerSubscribeBuilder>
    {
        /// <summary>
        /// Consumer's group name.
        /// </summary>
        /// <param name="consumerGroup">Name of the group.</param>
        /// <returns></returns>
        IConsumerSubscribeBuilder Group(string consumerGroup);

        /// <summary>
        /// Set the consumer's name
        /// </summary>
        IConsumerSubscribeBuilder Name(string consumerName);

        /// <summary>
        /// The routing information attached to this builder
        /// </summary>
        IPlanRoute Route { get; }

        /// <summary>
        /// Build receiver (on demand data query).
        /// </summary>
        /// <returns></returns>
        IConsumerReceiver BuildReceiver();

        /// <summary>
        /// Build iterator (pull fusion).
        /// </summary>
        /// <returns></returns>
        IConsumerIterator BuildIterator();


        /// <summary>
        /// Subscribe consumer.
        /// </summary>
        /// <param name="handlers">Per operation invocation handler, handle methods calls.</param>
        /// <returns>
        /// Remove subscription.
        /// keeping the disposable will prevent the consumer to be collected
        /// by th GC (when the behavior don't indicate to hook it until cancellation or dispose).
        /// </returns>
        IConsumerLifetime Subscribe(params ISubscriptionBridge[] handlers);

        /// <summary>
        /// Subscribe consumer.
        /// </summary>
        /// <param name="handlers">Per operation invocation handler, handle methods calls.</param>
        /// <returns>
        /// Remove subscription.
        /// keeping the disposable will prevent the consumer to be collected
        /// by th GC (when the behavior don't indicate to hook it until cancellation or dispose).
        /// </returns>
        IConsumerLifetime Subscribe(IEnumerable<ISubscriptionBridge> handlers);

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
            params Func<Announcement, IConsumerBridge, Task<bool>>[] handlers);

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
            IEnumerable<Func<Announcement, IConsumerBridge, Task<bool>>> handlers);

    }
}
