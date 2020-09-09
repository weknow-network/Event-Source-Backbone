using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Preform acknowledge (which should prevent the 
    /// message from process again by the consumer)
    /// </summary>
    /// <seealso cref="System.IAsyncDisposable" />
    public class AckOnce : IAck
    {
        private readonly Func<ValueTask> _ack;
        private readonly AckBehavior _behavior;
        private int _ackCount = 0;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="ack">The ack.</param>
        /// <param name="behavior">The behavior.</param>
        public AckOnce(
            Func<ValueTask> ack,
            AckBehavior behavior)
        {
            _ack = ack;
            _behavior = behavior;
        }

        #endregion // Ctor

        #region AckAsync

        /// <summary>
        /// Preform acknowledge (which should prevent the 
        /// message from process again by the consumer)
        /// </summary>
        /// <returns></returns>
        public async ValueTask AckAsync()
        {
            int count = Interlocked.Increment(ref _ackCount);
            if (count == 0)
                await _ack();
        }

        #endregion // AckAsync

        #region Cancel

        /// <summary>
        /// Cancel acknowledge (will happen on error in order to avoid ack on succeed)
        /// </summary>
        /// <returns></returns>
        public void Cancel()
        {
            Interlocked.Increment(ref _ackCount);
        }

        #endregion // Cancel

        #region DisposeAsync

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous dispose operation.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public async ValueTask DisposeAsync()
        {
            if (_behavior == AckBehavior.OnSucceed)
            {
                await AckAsync();
            }
        }

        #endregion // DisposeAsync
    }
}