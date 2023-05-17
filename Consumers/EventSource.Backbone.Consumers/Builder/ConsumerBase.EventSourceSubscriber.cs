using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

using EventSource.Backbone.Building;
using EventSource.Backbone.Enums;

using Handler = System.Func<EventSource.Backbone.Announcement, EventSource.Backbone.IConsumerBridge, System.Threading.Tasks.Task<bool>>;

namespace EventSource.Backbone
{

    /// <summary>
    /// The consumer base.
    /// </summary>
    public partial class ConsumerBase
    {
        /// <summary>
        /// Represent single consuming subscription
        /// </summary>
        private class EventSourceSubscriber : IConsumerLifetime, IConsumerBridge
        {
            private readonly IConsumer _consumer;
            private readonly CancellationTokenSource _disposeCancellation = new CancellationTokenSource();
            private readonly ValueTask _subscriptionLifetime;
            private readonly ConsumerOptions _options;
            private readonly uint _maxMessages;
            private readonly static ConcurrentDictionary<object, object?> _keepAlive = // prevent collection by the GC
                                                new ConcurrentDictionary<object, object?>();
            private readonly TaskCompletionSource<object?> _completion = new TaskCompletionSource<object?>();

            // counter of consuming attempt (either successful or faulted) not includes Polly retry policy
            private long _consumeCounter;
            private readonly ConcurrentQueue<Handler> _handlers;

            #region Ctor

            /// <summary>
            /// Initializes a new subscription.
            /// </summary>
            /// <param name="consumer">The consumer.</param>
            /// <param name="handlers">Per operation invocation handler, handle methods calls.</param>

            public EventSourceSubscriber(
                IConsumer consumer,
                IEnumerable<Handler> handlers)
            {
                var plan = consumer.Plan;
                var channel = plan.Channel;
                _consumer = consumer;
                if (plan.Options.KeepAlive)
                    _keepAlive.TryAdd(this, null);

                _options = Plan.Options;
                _maxMessages = _options.MaxMessages;
                _handlers = new ConcurrentQueue<Handler>(handlers);

                _subscriptionLifetime = channel.SubsribeAsync(
                                                    plan,
                                                    ConsumingAsync,
                                                    _disposeCancellation.Token);

                _subscriptionLifetime.AsTask().ContinueWith(_ => DisposeAsync());
            }

            #endregion // Ctor

            private IConsumerPlan Plan => _consumer.Plan;
            private ILogger Logger => Plan.Logger;

            #region GetParameterAsync

            /// <summary>
            /// Get parameter value from the announcement.
            /// </summary>
            /// <typeparam name="TParam">The type of the parameter.</typeparam>
            /// <param name="arg">The argument.</param>
            /// <param name="argumentName">Name of the argument.</param>
            /// <returns></returns>
            /// <exception cref="NotSupportedException"></exception>
            ValueTask<TParam> IConsumerBridge.GetParameterAsync<TParam>(Announcement arg, string argumentName) => Plan.GetParameterAsync<TParam>(arg, argumentName);

            #endregion // GetParameterAsync

            #region Completion

            /// <summary>
            /// Represent the consuming completion..
            /// </summary>
            public Task Completion => _completion.Task;

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
            private async ValueTask<bool> ConsumingAsync(
                Announcement arg,
                IAck ack)
            {
                #region Validation

                if (_handlers == null)
                {
                    var err = "Must supply non-null instance for the subscription";
                    var ex = new ArgumentNullException(err);
                    Logger.LogWarning(ex, err);
                    throw ex;
                }

                #endregion // Validation

                CancellationToken cancellation = Plan.Cancellation;
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

                var logger = Logger;
                logger.LogDebug("Consuming event: {0}", meta.Key());

                #region _plan.Interceptors.InterceptAsync(...)

                Bucket interceptionBucket = arg.InterceptorsData;
                foreach (var interceptor in Plan.Interceptors)
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
                PartialConsumerBehavior partialBehavior = _options.PartialBehavior;

                bool hasProcessed = false;
                try
                {

                    await using (Ack.Set(ack))
                    {
                        hasProcessed = await Plan.ResiliencePolicy.ExecuteAsync<bool>(async (ct) =>
                        {
                            if (ct.IsCancellationRequested) return false;

                            ConsumerMetadata._metaContext.Value = consumerMeta;
                            if (_options.MultiConsumerBehavior == MultiConsumerBehavior.All)
                            {
                                var tasks = _handlers.AsParallel().Select(h => h.Invoke(arg, this));
                                var results = await Task.WhenAll(tasks);
                                return results?.Any(m => m) ?? false;
                            }
                            foreach (Handler handler in _handlers)
                            {
                                if (await handler.Invoke(arg, this))
                                    return true;
                            }
                            return false;
                        }, cancellation);
                        logger.LogDebug("Consumed event: {0}", meta.Key());

                        var options = Plan.Options;
                        var behavior = options.AckBehavior;
                        if (partialBehavior == PartialConsumerBehavior.ThrowIfNotHandled && !hasProcessed)
                        {
                            Logger.LogCritical("No handler is matching event: {stream}, operation{operation}, MessageId:{id}", meta.Key(), meta.Operation, meta.MessageId);
                            throw new InvalidOperationException($"No handler is matching event: {meta.Key()}, operation{meta.Operation}, MessageId:{meta.MessageId}");
                        }
                        if (hasProcessed)
                        {
                            if (behavior == AckBehavior.OnSucceed)
                                await ack.AckAsync();
                        }
                        else
                        {
                            if (partialBehavior == PartialConsumerBehavior.Loose)
                            {
                                await ack.AckAsync();
                            }
                            if (partialBehavior == PartialConsumerBehavior.Sequential)
                            {
                                await ack.AckAsync();
                            }
                        }
                    }
                }
                #region Exception Handling

                catch (OperationCanceledException)
                {
                    logger.LogWarning("Canceled event: {0}", meta.Key());
                    if (Plan.Options.AckBehavior != AckBehavior.OnFinally)
                    {
                        await ack.CancelAsync();
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "event: {0}", meta.Key());
                    if (Plan.Options.AckBehavior != AckBehavior.OnFinally)
                    {
                        await ack.CancelAsync();
                        throw;
                    }
                }

                #endregion // Exception Handling
                finally
                {

                    if (Plan.Options.AckBehavior == AckBehavior.OnFinally)
                    {
                        if (partialBehavior != PartialConsumerBehavior.Sequential)
                        {
                            await ack.AckAsync();
                        }
                    }

                    #region Validation Max Messages Limit

                    if (_maxMessages != 0 && _maxMessages <= count)
                    {
                        await DisposeAsync();
                    }

                    #endregion // Validation Max Messages Limit
                }

                return hasProcessed;
            }

            #endregion // ConsumingAsync

            #region Subscribe

            /// <summary>
            /// Subscribe consumer.
            /// </summary>
            /// <param name="handlers">Per operation invocation handler, handle methods calls.</param>
            /// <returns>
            /// The partition subscription (dispose to remove the subscription)
            /// </returns>
            /// <exception cref="System.ArgumentNullException">_plan</exception>
            IConsumerLifetime IConsumerSubscribtionHubBuilder.Subscribe(ISubscriptionBridge[] handlers)

            {
                return ((IConsumerSubscribtionHubBuilder)this).Subscribe(handlers as IEnumerable<ISubscriptionBridge>);
            }

            /// <summary>
            /// Subscribe consumer.
            /// </summary>
            /// <param name="handlers">Per operation invocation handler, handle methods calls.</param>
            /// <returns>
            /// The partition subscription (dispose to remove the subscription)
            /// </returns>
            /// <exception cref="System.ArgumentNullException">_plan</exception>
            IConsumerLifetime IConsumerSubscribtionHubBuilder.Subscribe(
                IEnumerable<ISubscriptionBridge> handlers)

            {
                IEnumerable<Handler> dels = handlers.Select<ISubscriptionBridge, Handler>(m => m.BridgeAsync);
                return ((IConsumerSubscribtionHubBuilder)this).Subscribe(dels);
            }

            /// <summary>
            /// Subscribe consumer.
            /// </summary>
            /// <param name="handlers">Per operation invocation handler, handle methods calls.</param>
            /// <returns>
            /// The partition subscription (dispose to remove the subscription)
            /// </returns>
            /// <exception cref="System.ArgumentNullException">_plan</exception>
            IConsumerLifetime IConsumerSubscribtionHubBuilder.Subscribe(
                params Handler[] handlers)

            {
                return ((IConsumerSubscribtionHubBuilder)this).Subscribe(handlers as IEnumerable<Handler>);
            }

            /// <summary>
            /// Subscribe consumer.
            /// </summary>
            /// <param name="handlers">Per operation invocation handler, handle methods calls.</param>
            /// <returns>
            /// The partition subscription (dispose to remove the subscription)
            /// </returns>
            /// <exception cref="System.ArgumentNullException">_plan</exception>
            IConsumerLifetime IConsumerSubscribtionHubBuilder.Subscribe(
                IEnumerable<Handler> handlers)

            {
                foreach (Handler handler in handlers)
                {
                    _handlers.Enqueue(handler);
                }
                return this;
            }

            #endregion // Subscribe

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

                if (_disposeCancellation.IsCancellationRequested)
                    return ValueTask.CompletedTask;

                #endregion // Validation

                _disposeCancellation.CancelSafe();
                _keepAlive.TryRemove(this, out _);
                _completion.SetResult(null);
                return _subscriptionLifetime;
            }

            #endregion // DisposeAsync
        }
    }
}
