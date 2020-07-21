using System;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Represent acknowledge-able instance.
    /// </summary>
    public class Ackable<T>
    {
        private readonly Func<ValueTask> _ack;

        #region Ctor

        /// <summary>
        /// Initializes a new instance        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="ackAsync">The acknowledge handle (callback).</param>
        public Ackable(
            T item, 
            Func<ValueTask> ackAsync)
        {
            Item = item;
            _ack = ackAsync;
        }

        #endregion // Ctor

        #region Item

        /// <summary>
        /// Gets the item data.
        /// </summary>
        public T Item { get; }

        #endregion // Item

        #region AckAsync

        /// <summary>
        /// Send Acknowledge (some queue type [like event sourcing] 
        /// keep the item in the queue until it processed,
        /// the acknowledge will notify it that it can be delete).
        /// </summary>
        /// <returns></returns>
        public ValueTask AckAsync() => _ack();

        #endregion // AckAsync
    }
}
