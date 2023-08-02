using EventSourcing.Backbone.UnitTests.Entities;

namespace EventSourcing.Backbone
{

    public class SimpleEventSubscriptionFromGen : SimpleEventConsumerBridgeBase
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target">The channel.</param>
        public SimpleEventSubscriptionFromGen(ISimpleEventConsumer target): base(target)
        {
        }

        #endregion // Ctor

        public override ValueTask ExecuteAsync(ConsumerMetadata consumerMetadata, string key, int value) => _targets[0].ExecuteAsync(consumerMetadata, key, value);

        public override ValueTask RunAsync(ConsumerMetadata consumerMetadata, int id, DateTime date) => _targets[0].RunAsync(consumerMetadata, id, date);

        public override ValueTask RunAsync(ConsumerMetadata consumerMetadata, int i)
        {
            throw new NotImplementedException();
        }

        public override ValueTask RunAsync(ConsumerMetadata consumerMetadata, TimeSpan ts)
        {
            throw new NotImplementedException();
        }
    }
}
