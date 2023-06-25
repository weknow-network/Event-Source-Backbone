﻿using System;
using System.Diagnostics.Metrics;

using Microsoft.Extensions.Logging;

using static EventSourcing.Backbone.Private.EventSourceTelemetry;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Preform acknowledge (which should prevent the 
    /// message from process again by the consumer)
    /// </summary>
    /// <seealso cref="System.IAsyncDisposable" />
    public class AckOnce : IAck
    {
        private static readonly Func<AckBehavior, ValueTask> NON_FN = (_) => ValueTask.CompletedTask;
        private readonly string _uri;
        private readonly Func<AckBehavior, ValueTask> _ackAsync;
        private readonly Func<AckBehavior, ValueTask> _cancelAsync;
        private readonly AckBehavior _behavior;
        private readonly ILogger _logger;
        private int _ackCount = 0;
        private static readonly Counter<int> AckCounter = EMeter.CreateCounter<int>("evt-src.sys.consumer.event.ack", "count",
                                                    "Event's message handling acknowledge count");
        private static readonly Counter<int> AbortCounter = EMeter.CreateCounter<int>("evt-src.sys.consumer.event.abort", "count",
                                                    "Event's message handling aborted (cancel) count");

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="ackAsync">The ack.</param>
        /// <param name="behavior">The behavior.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="cancelAsync">The cancel.</param>
        public AckOnce(
            string uri,
            Func<AckBehavior, ValueTask> ackAsync,
            AckBehavior behavior,
            ILogger logger,
            Func<AckBehavior, ValueTask>? cancelAsync = null)
        {
            _uri = uri;
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
        /// <param name="cause">The cause of the acknowledge.</param>
        /// <returns></returns>
        public async ValueTask AckAsync(AckBehavior cause)
        {
            int count = Interlocked.Increment(ref _ackCount);
            if (count != 1)
                return;
            try
            {
                AckCounter.WithTag("URI", _uri).WithTag("cause", cause).Add(1);
                await _ackAsync(cause);
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
        /// <param name="cause">The cause of the cancellation.</param>
        /// <returns></returns>
        /// Must be execute from a consuming scope (i.e. method call invoked by the consumer's event processing)
        public async ValueTask CancelAsync(AckBehavior cause)
        {
            int count = Interlocked.Increment(ref _ackCount);
            if (count != 1)
                return;
            try
            {
                AbortCounter.WithTag("URI", _uri).WithTag("cause", cause).Add(1);
                await _cancelAsync(cause);
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
                await AckAsync(AckBehavior.OnSucceed);
            }
        }

        #endregion // DisposeAsync
    }
}