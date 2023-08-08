using Microsoft.Extensions.Logging;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Event Source raw producer.
    /// </summary>
    public interface IRawProducer
    {
        /// <summary>
        /// <![CDATA[Producer proxy for raw events sequence.
        /// Useful for data migration at the raw data level.]]>
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        ValueTask Produce(Announcement data, ILogger logger);

        /// <summary>
        /// Converts to a subscription bridge which will forward the data into the producer when attached to a subscriber.
        /// </summary>
        /// <returns></returns>
        public ISubscriptionBridge ToSubscriptionBridge()
        {
            return new SubscriptionBridge(this);
        }

        #region class SubscriptionBridge : ISubscriptionBridge ...

        /// <summary>
        /// Subscription Bridge which produce forward data
        /// </summary>
        /// <seealso cref="EventSourcing.Backbone.ISubscriptionBridge" />
        private sealed class SubscriptionBridge : ISubscriptionBridge
        {
            private readonly IRawProducer _fw;

            public SubscriptionBridge(IRawProducer fw)
            {
                _fw = fw;
            }

            async Task<bool> ISubscriptionBridge.BridgeAsync(Announcement announcement, IConsumerBridge consumerBridge, IPlanBase plan)
            {
                await _fw.Produce(announcement, plan.Logger);
                return true;
            }
        }

        #endregion // class SubscriptionBridge : ISubscriptionBridge ...
    }
}
