﻿namespace EventSourcing.Backbone.Building
{
    public interface IConsumerSubscriptionHubBuilder
    {
        /// <summary>
        /// Subscribe consumer.
        /// </summary>
        /// <param name="handler">Per operation invocation handler, handle methods calls.</param>
        /// <returns>
        /// Remove subscription.
        /// keeping the disposable will prevent the consumer to be collected
        /// by th GC (when the behavior don't indicate to hook it until cancellation or dispose).
        /// </returns>
        IConsumerLifetime Subscribe(ISubscriptionBridge handler);
    }
}
