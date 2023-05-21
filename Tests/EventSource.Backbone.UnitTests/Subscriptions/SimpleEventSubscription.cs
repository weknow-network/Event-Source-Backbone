using EventSource.Backbone.UnitTests.Entities;

namespace EventSource.Backbone
{

    /// <summary>
    /// In-Memory Channel (excellent for testing)
    /// </summary>
    /// <seealso cref="EventSource.Backbone.IProducerChannelProvider" />
    public class SimpleEventSubscription : SimpleEventSubscriptionBase
    {
        private readonly ISimpleEventConsumer _target;

        #region Ctor

        public SimpleEventSubscription(ISimpleEventConsumer target)
        {
            _target = target;
        }

        #endregion // Ctor

        protected override ValueTask ExecuteAsync(string key, int value) => _target.ExecuteAsync(key, value);

        protected override ValueTask RunAsync(int id, DateTime date) => _target.RunAsync(id, date);
    }
}
