using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Represent acknowledge-able instance.
    /// </summary>
    [DebuggerDisplay("DespatchMessage: {MessageId}")]
    public class AckableCollection<T>
    {
        private readonly Func<IEnumerable<T>, Task> _ack;
        private int _counter;
        // each child acknowledge entered to the queue
        private readonly ConcurrentQueue<T> _acksBy = new ConcurrentQueue<T>();

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="Ackable{T}" /> class.
        /// </summary>
        /// <param name="count">
        /// The count of the items.
        /// will propagate acknowledge after count times of acknowledges.
        /// </param>
        /// <param name="ackAsync">The acknowledge handle (callback).</param>
        public AckableCollection(
            int count,
            Func<IEnumerable<T>, Task> ackAsync)
        {
            _counter = count;
            _ack = ackAsync;
        }

        #endregion // Ctor

        #region Item

        /// <summary>
        /// Gets the item data.
        /// </summary>
        public Ackable<T> AddItem(T item)
        {
            return new Ackable<T>(item, AckCallbackAsync);
        }

        #endregion // Item

        #region AckCallbackAsync

        /// <summary>
        /// Acknowledge callback.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private Task AckCallbackAsync(T item)
        {
            _acksBy.Enqueue(item);
            var count = Interlocked.Decrement(ref _counter);
            if (count == 0)
                _ack(_acksBy);
            return Task.CompletedTask;
        }

        #endregion // AckCallbackAsync
    }
}
