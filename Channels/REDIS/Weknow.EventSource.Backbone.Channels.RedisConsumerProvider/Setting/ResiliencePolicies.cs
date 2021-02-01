
using System;
using Microsoft.Extensions.Logging;
using Polly;
using System.Threading.Tasks;
using Polly.CircuitBreaker;

// TODO: [bnaya 2021-02] enable interception func

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    /// <summary>
    /// Define when to claim stale (long waiting) messages from other consumers
    /// </summary>
    public class ResiliencePolicies
    {
        public static readonly ResiliencePolicies Empty = new ResiliencePolicies();

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="onBreak">The on break.</param>
        /// <param name="onReset">The on reset.</param>
        /// <param name="onHalfOpen">The on half open.</param>
        /// <param name="onRetry">The on retry.</param>
        public ResiliencePolicies(
            Action<Exception, CircuitState, TimeSpan, Context>? onBreak = null,
            Action<Context>? onReset = null,
            Action? onHalfOpen = null,
            Func<Exception, TimeSpan, int, Context, Task>? onRetry = null
            )
        {
            var onBreak_ = (onBreak ?? ((ex, state, duration, ctx) => { }));
            var onReset_ = onReset ?? ((Action<Context>)((ctx) => { }));
            var onHalfOpen_ = onHalfOpen ?? (() => { });
            var onRetry_ = onRetry ?? ((ex, duration, retryCount, ctx) => Task.CompletedTask);

            //_onBreak = onBreak_;
            //_onReset = onReset_;
            //_onHalfOpen = onHalfOpen_;
            //_onRetry = onRetry_;

            //BatchReading = DefaultReadingPolicy;

            AsyncPolicy retryReading = Policy.Handle<Exception>()
                  .RetryForeverAsync((ex, i, c) => onRetry_(ex, TimeSpan.Zero, i, c));

            AsyncPolicy retryInvocation = Policy.Handle<Exception>()
                  .WaitAndRetryAsync(3, 
                        (retryCount) => TimeSpan.FromSeconds(Math.Pow(2, retryCount)),
                        onRetry_); 
            
            AsyncPolicy breaker = Policy.Handle<Exception>()
                            //.CircuitBreakerAsync(
                            //                10,
                                //                TimeSpan.FromSeconds(20),
                            //                onBreak_,
                            //                onReset_,
                            //                onHalfOpen_);
                            .AdvancedCircuitBreakerAsync(
                                failureThreshold: 0.5, // Break on >=50% actions result in handled exceptions...
                                samplingDuration: TimeSpan.FromSeconds(20), // ... over any 10 second period
                                minimumThroughput: 8, // ... provided at least 8 actions in the 10 second period.
                                durationOfBreak: TimeSpan.FromSeconds(30), // Break for 30 seconds.
                                onBreak_,
                                onReset_,
                                onHalfOpen_);

            BatchReading = retryReading.WrapAsync(breaker);
            Invocation = retryInvocation; 
        }

        #endregion // Ctor

        /// <summary>
        /// Gets or sets the batch reading policy.
        /// </summary>
        public AsyncPolicy BatchReading { get; set; }

        /// <summary>
        /// Gets or sets the invocation policy.
        /// </summary>
        public AsyncPolicy Invocation { get; set; }
    }
}