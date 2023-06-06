using EventSourcing.Backbone.UnitTests.Entities;
using EventSourcing.Backbone.UnitTests.Entities.Hidden;

namespace EventSourcing.Backbone
{

    public class SimpleEventSubscriptionFromGen : SimpleEventConsumerBase
    {
        private readonly ISimpleEventConsumer _target;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target">The channel.</param>
        public SimpleEventSubscriptionFromGen(ISimpleEventConsumer target)
        {
            _target = target;
        }

        #endregion // Ctor

        protected override ValueTask ExecuteAsync(ConsumerMetadata consumerMetadata, string key, int value) => _target.ExecuteAsync(consumerMetadata, key, value);

        protected override ValueTask RunAsync(ConsumerMetadata consumerMetadata, int id, DateTime date) => _target.RunAsync(consumerMetadata, id, date);
    }
}
