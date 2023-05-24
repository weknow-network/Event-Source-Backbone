using Microsoft.Extensions.Logging;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Preform acknowledge (which should prevent the 
    /// message from process again by the consumer)
    /// </summary>
    /// <seealso cref="System.IAsyncDisposable" />
    public class AckOnce : IAck
    {
        private static readonly Func<ValueTask> NON_FN = () => ValueTask.CompletedTask;
        private readonly Func<ValueTask> _ackAsync;
        private readonly Func<ValueTask> _cancelAsync;
        private readonly AckBehavior _behavior;
        private readonly ILogger _logger;
        private int _ackCount = 0;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="ackAsync">The ack.</param>
        /// <param name="cancelAsync">The cancel.</param>
        /// <param name="behavior">The behavior.</param>
        /// <param name="logger">The logger.</param>
        public AckOnce(
            Func<ValueTask> ackAsync,
            AckBehavior behavior,
            ILogger logger,
            Func<ValueTask>? cancelAsync = null)
        {
            _ackAsync = ackAsync;
            _cancelAsync = cancelAsync ?? NON_FN;
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
                    await _ackAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ack failure: Behavior = {behavior}", _behavior);
                Interlocked.Decrement(ref _ackCount);
                throw;
            }
        }

        #endregion // AckAsync

        // TODO: [bnaya 2020-10] cancel pending
        #region CancelAsync

        /// <summary>
        /// Cancel acknowledge (will happen on error in order to avoid ack on succeed)
        /// </summary>
        /// <returns></returns>
        public async ValueTask CancelAsync()
        {
            int count = Interlocked.Increment(ref _ackCount);
            try
            {
                if (count == 1)
                    await _cancelAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cancel failure: Behavior = {behavior}", _behavior);
                Interlocked.Decrement(ref _ackCount);
                throw;
            }

        }

        #endregion // CancelAsync

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