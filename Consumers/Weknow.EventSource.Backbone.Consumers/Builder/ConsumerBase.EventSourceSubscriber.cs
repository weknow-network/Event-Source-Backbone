using Microsoft.Extensions.Logging;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{

    /// <summary>
    /// The consumer base.
    /// </summary>
    partial class ConsumerBase
    {
        /// <summary>
        /// Represent single consuming subscription
        /// </summary>
        private class EventSourceSubscriber : IConsumerLifetime, IConsumerBridge
        {
            private readonly IConsumerPlan _plan;
            private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
            private readonly ValueTask _subscriptionLifetime;
            private readonly ConsumerOptions _options;
            private readonly uint _maxMessages;
            private readonly static ConcurrentDictionary<object, object?> _keepAlive = // prevent collection by the GC
                                                new ConcurrentDictionary<object, object?>();
            private readonly TaskCompletionSource<object?> _completion = new TaskCompletionSource<object?>();

            // counter of consuming attempt (either successful or faulted) not includes Polly retry policy
            private long _consumeCounter;
            private readonly IEnumerable<Func<Announcement, IConsumerBridge, Task>> _handlers;

            #region Ctor

            /// <summary>
            /// Initializes a new subscription.
            /// </summary>
            /// <param name="plan">The plan.</param>
            /// <param name="handlers">Per operation invocation handler, handle methods calls.</param>

            public EventSourceSubscriber(
                IConsumerPlan plan,
                IEnumerable<Func<Announcement, IConsumerBridge, Task>> handlers)
            {
                var channel = plan.Channel;
                _plan = plan;
                if (plan.Options.KeepAlive)
                    _keepAlive.TryAdd(this, null);

                _options = _plan.Options;
                _maxMessages = _options.MaxMessages;
                _handlers = handlers;

                _subscriptionLifetime = channel.SubsribeAsync(
                                                    plan,
                                                    ConsumingAsync,
                                                    plan.Options,
                                                    _cancellation.Token);

                _subscriptionLifetime.AsTask().ContinueWith(_ => DisposeAsync());
            }

            #endregion // Ctor

            #region GetParameterAsync

            /// <summary>
            /// Get parameter value from the announcement.
            /// </summary>
            /// <typeparam name="TParam">The type of the parameter.</typeparam>
            /// <param name="arg">The argument.</param>
            /// <param name="argumentName">Name of the argument.</param>
            /// <returns></returns>
            /// <exception cref="NotSupportedException"></exception>
            ValueTask<TParam> IConsumerBridge.GetParameterAsync<TParam>(Announcement arg, string argumentName) => _plan.GetParameterAsync<TParam>(arg, argumentName);

            #endregion // GetParameterAsync

            #region Completion
#pragma warning disable AMNF0001 // Asynchronous method name is not ending with 'Async'

            /// <summary>
            /// Represent the consuming completion..
            /// </summary>
            public Task Completion => _completion.Task;

#pragma warning restore AMNF0001 // Asynchronous method name is not ending with 'Async'
            #endregion // Completion

            #region ConsumingAsync

            /// <summary>
            /// Handles consuming of single event.
            /// </summary>
            /// <param name="arg">The argument.</param>
            /// <param name="ack">
            /// The acknowledge callback which will prevent message from 
            /// being re-fetch from same consumer group.</param>
            /// <returns></returns>
            private async ValueTask ConsumingAsync(
                Announcement arg,
                IAck ack)
            {
                #region Validation

                if (_handlers == null)
                {
                    var err = "Must supply non-null instance for the subscription";
                    var ex = new ArgumentNullException(err);
                    _plan.Logger?.LogWarning(ex, err);
                    throw ex;
                }

                #endregion // Validation

                CancellationToken cancellation = _plan.Cancellation;
                Metadata meta = arg.Metadata;

                #region Increment & Validation Max Messages Limit

                long count = Interlocked.Increment(ref _consumeCounter);
                if (_maxMessages != 0 && _maxMessages < count)
                {
                    await DisposeAsync();
                    throw new OperationCanceledException(); // make sure it not auto ack;
                }

                #endregion // Increment & Validation Max Messages Limit

                var consumerMeta = new ConsumerMetadata(meta, cancellation);

                var logger = _plan.Logger;
                logger.LogDebug("Consuming event: {0}", meta.Key());

                #region _plan.Interceptors.InterceptAsync(...)

                Bucket interceptionBucket = arg.InterceptorsData;
                foreach (var interceptor in _plan.Interceptors)
                {
                    if (!interceptionBucket.TryGetValue(
                                            interceptor.InterceptorName,
                                            out ReadOnlyMemory<byte> interceptedData))
                    {
                        interceptedData = ReadOnlyMemory<byte>.Empty;
                    }
                    await interceptor.InterceptAsync(meta, interceptedData);
                }

                #endregion // _plan.Interceptors.InterceptAsync(...)

                string operation = meta.Operation;

                try
                {
                    await using (Ack.Set(ack))
                    {
                        await _plan.ResiliencePolicy.ExecuteAsync(async () =>
                        {
                            ConsumerMetadata._metaContext.Value = consumerMeta;
                            var tasks = _handlers.Select(h => h.Invoke(arg, this));
                            await Task.WhenAll(tasks);
                        });
                    }
                    logger.LogDebug("Consumed event: {0}", meta.Key());

                    var behavior = _plan.Options.AckBehavior;
                    if (behavior == AckBehavior.OnSucceed)
                        await ack.AckAsync();
                }
                #region Exception Handling

                catch (OperationCanceledException)
                {
                    logger.LogWarning("Canceled event: {0}", meta.Key());
                    if (_plan.Options.AckBehavior != AckBehavior.OnFinally)
                    {
                        await ack.CancelAsync();
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "event: {0}", meta.Key());
                    if (_plan.Options.AckBehavior != AckBehavior.OnFinally)
                    {
                        await ack.CancelAsync();
                        throw;
                    }
                }

                #endregion // Exception Handling
                finally
                {
                    if (_plan.Options.AckBehavior == AckBehavior.OnFinally)
                        await ack.AckAsync();

                    #region Validation Max Messages Limit

                    if (_maxMessages != 0 && _maxMessages <= count)
                    {
                        await DisposeAsync();
                    }

                    #endregion // Validation Max Messages Limit
                }
            }

            #endregion // ConsumingAsync

            #region DisposeAsync

            /// <summary>
            /// Release consumer.
            /// </summary>
            /// <returns>
            /// A task that represents the asynchronous dispose operation.
            /// </returns>
            public ValueTask DisposeAsync()
            {
                #region Validation

                if (_cancellation.IsCancellationRequested)
                    return ValueTask.CompletedTask;

                #endregion // Validation

                _cancellation.CancelSafe();
                _keepAlive.TryRemove(this, out _);
                _completion.SetResult(null);
                return _subscriptionLifetime;
            }

            #endregion // DisposeAsync
        }
    }
}
