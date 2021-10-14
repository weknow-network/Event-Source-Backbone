
using System;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Base class for the consumer's code generator
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class ConsumerBase<T>
    {
        private readonly IConsumerPlan _plan;
        private readonly T _handler;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="handler">The events handler.</param>
        public ConsumerBase(
            IConsumerPlanBuilder plan,
            T handler)
        {
            _plan = plan.Build();
            _handler = handler;
        }

        #endregion // Ctor

        #region Subscribe

        /// <summary>
        /// Subscribes this instance.
        /// </summary>
        /// <returns></returns>
        public IConsumerLifetime Subscribe()
        {
            var subscription = new EventSourceSubscriberBase<T>(_plan, _handler);
            return subscription;
        }

        #endregion // Subscribe
    }
}
