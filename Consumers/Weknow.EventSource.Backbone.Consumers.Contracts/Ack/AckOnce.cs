using Microsoft.Extensions.Logging;

using System;
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
        private readonly ILogger _logger;
        private int _ackCount = 0;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="ack">The ack.</param>
        /// <param name="behavior">The behavior.</param>
        /// <param name="logger">The logger.</param>
        public AckOnce(
            Func<ValueTask> ack,
            AckBehavior behavior,
            ILogger logger)
        {
            _ack = ack;
            _behavior = behavior;
            _logger = logger;
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
            try
            {
                if (count == 1)
                    await _ack();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ack failure: Behavior = {behavior}", _behavior);
                Interlocked.Decrement(ref _ackCount);
                throw;
            }
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