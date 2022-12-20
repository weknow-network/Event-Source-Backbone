using Weknow.EventSource.Backbone.UnitTests.Entities;
using Weknow.EventSource.Backbone.UnitTests.Entities.Hidden;

namespace Weknow.EventSource.Backbone
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

        protected override ValueTask ExecuteAsync(string key, int value) => _target.ExecuteAsync(key, value);

        protected override ValueTask RunAsync(int id, DateTime date) => _target.RunAsync(id, date);
    }
}
