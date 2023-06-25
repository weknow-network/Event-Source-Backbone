﻿namespace EventSourcing.Backbone
{
    /// <summary>
    /// Base class for the consumer's code generator
    /// </summary>
    public partial class ConsumerBase : IConsumer
    {
        private readonly IConsumerPlan _plan;
        private readonly IEnumerable<Func<Announcement, IConsumerBridge, Task<bool>>> _handlers;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="handlers">Per method handler, handle methods calls.</param>
        public ConsumerBase(
            IConsumerPlanBuilder plan,
            IEnumerable<Func<Announcement, IConsumerBridge, Task<bool>>> handlers)
        {
            _plan = plan.Build();
            _handlers = handlers;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="handlers">Per method handler, handle methods calls.</param>
        public ConsumerBase(
            IConsumerPlanBuilder plan,
            IEnumerable<ISubscriptionBridge> handlers)
        {
            _plan = plan.Build();
            _handlers = handlers.Select<ISubscriptionBridge, Func<Announcement, IConsumerBridge, Task<bool>>>(m => m.BridgeAsync);
        }

        #endregion // Ctor

        #region Subscribe

        /// <summary>
        /// Subscribes this instance.
        /// </summary>
        /// <returns></returns>
        public IConsumerLifetime Subscribe()
        {
            var subscription = new EventSourceSubscriber(this, _handlers);
            return subscription;
        }

        #endregion // Subscribe

        #region Plan

        /// <summary>
        /// Gets the plan.
        /// </summary>
        IConsumerPlan IConsumer.Plan => _plan;

        #endregion // Plan
    }
}
