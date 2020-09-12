
using System;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Base class for the consumer's code generator
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class ConsumerBase<T>
    {
        private readonly ConsumerPlan _plan;
        private readonly Func<ConsumerMetadata, T> _factory;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="factory">The factory.</param>
        public ConsumerBase(
            ConsumerPlan plan,
            Func<ConsumerMetadata, T> factory)
        {
            _plan = plan;
            _factory = factory;
        }

        #endregion // Ctor

        #region Subscribe

        /// <summary>
        /// Subscribes this instance.
        /// </summary>
        /// <returns></returns>
        public IConsumerLifetime Subscribe()
        {
            var subscription = new Subscription(_plan, _factory);
            return subscription;
        }

        #endregion // Subscribe
    }
}
