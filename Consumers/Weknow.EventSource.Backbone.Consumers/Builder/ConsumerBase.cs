
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Base class for the consumer's code generator
    /// </summary>
    public partial class ConsumerBase
    {
        private readonly IConsumerPlan _plan;
        private readonly Func<Announcement, IConsumerBridge, Task>[] _handlers;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="handlers">Per method handler, handle methods calls.</param>
        public ConsumerBase(
            IConsumerPlanBuilder plan,
            Func<Announcement, IConsumerBridge, Task>[] handlers)
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
            ISubscriptionBridge[] handlers)
        {
            _plan = plan.Build();
            _handlers = handlers.Select<ISubscriptionBridge, Func<Announcement, IConsumerBridge, Task>>(m => m.BridgeAsync).ToArray();
        }

        #endregion // Ctor

        #region Subscribe

        /// <summary>
        /// Subscribes this instance.
        /// </summary>
        /// <returns></returns>
        public IConsumerLifetime Subscribe()
        {
            var subscription = new EventSourceSubscriber(_plan, _handlers);
            return subscription;
        }

        #endregion // Subscribe
    }
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
            var subscription = new Subscription(_plan, _handler);
            return subscription;
        }

        #endregion // Subscribe
    }
}
