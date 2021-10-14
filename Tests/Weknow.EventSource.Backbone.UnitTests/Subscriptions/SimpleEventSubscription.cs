using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.UnitTests.Entities;

namespace Weknow.EventSource.Backbone
{

    /// <summary>
    /// In-Memory Channel (excellent for testing)
    /// </summary>
    /// <seealso cref="Weknow.EventSource.Backbone.IProducerChannelProvider" />
    public class SimpleEventSubscription : SimpleEventSubscriptionBase
    {
        private readonly ISimpleEventConsumer _target;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="channel">The channel.</param>
        public SimpleEventSubscription(ISimpleEventConsumer target)
        {
            _target = target;
        }

        #endregion // Ctor

        protected override ValueTask ExecuteAsync(string key, int value) => _target.ExecuteAsync(key, value);

        protected override ValueTask RunAsync(int id, DateTime date) => _target.RunAsync(id, date);
    }
}
