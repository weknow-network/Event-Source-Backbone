using Microsoft.Extensions.Logging;

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    partial class ConsumerBase<T>
    {

        /// <summary>
        /// Represent single consuming subscription
        /// </summary>
        private class Subscription : IConsumerLifetime
        {
            private readonly IConsumerPlan _plan;
            private readonly Func<ConsumerMetadata, T> _factory;
            private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
            private readonly ValueTask _subscriptionLifetime;
            private readonly IEventSourceConsumerOptions _options;
            private readonly uint _maxMessages;
            private readonly static ConcurrentDictionary<Subscription, object?> _keepAlive =
                                                new ConcurrentDictionary<Subscription, object?>();
            private readonly TaskCompletionSource<object?> _completion = new TaskCompletionSource<object?>();
            private long _consumeCounter;

            #region Ctor

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="plan">The plan.</param>
            /// <param name="factory">The factory.</param>
            public Subscription(
                IConsumerPlan plan,
                Func<ConsumerMetadata, T> factory)
            {
                var channel = plan.Channel;
                _plan = plan;
                _factory = factory;
                if (plan.Options.KeepAlive)
                    _keepAlive.TryAdd(this, null);

                _options = _plan.Options;
                _maxMessages = _options.MaxMessages;

                _subscriptionLifetime = channel.SubsribeAsync(
                                                    plan,
                                                    ConsumingAsync,
                                                    plan.Options,
                                                    _cancellation.Token);
            }

            #endregion // Ctor


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

                CancellationToken cancellation = _plan.Cancellation;
                Metadata meta = arg.Metadata;

                #region Validation Max Messages Limit

                long count = Interlocked.Increment(ref _consumeCounter);
                if (_maxMessages != 0 && _maxMessages < count)
                {
                    await DisposeAsync();
                    throw new OperationCanceledException(); // make sure it not auto ack;
                }

                #endregion // Validation Max Messages Limit

                var consumerMeta = new ConsumerMetadata(meta, cancellation);
                T instance = _factory(consumerMeta);
                #region Validation

                if (instance == null)
                    throw new NullReferenceException("_factory(consumerMeta)");

                #endregion // Validation

                var logger = _plan.Logger;
                logger.LogDebug("Consuming event: {0}", meta.Key);

                #region Validation

                if (instance == null)
                {
                    var ex = new ArgumentNullException("Cannot create instance");
                    _plan.Logger?.LogWarning(ex, "Consumer fail to create instance");
                    throw ex;
                }

                #endregion // Validation

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

                #region MethodInfo? method = type.GetMethod(operation)

                string operation = arg.Metadata.Operation;
                Type type = instance.GetType();
                var binding = BindingFlags.Public | BindingFlags.Instance;
                MethodInfo? method = type.GetMethod(operation, binding);
                #region Validation

                if (method == null)
                {
                    var ex = new ArgumentNullException("Cannot get method");
                    _plan.Logger?.LogWarning(ex, "Consumer fail to get method");
                    throw ex;
                }

                #endregion // Validation

                #endregion // MethodInfo? method = type.GetMethod(operation)

                ParameterInfo[] parameters = method.GetParameters();

                #region object[] arguments = ... unclassify

                object[] arguments = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    #region Unclassify(segmentations)

                    ParameterInfo? parameter = parameters[i];
                    MethodInfo? unclassify = this.GetType().GetMethod(nameof(UnclassifyAsync), BindingFlags.NonPublic | BindingFlags.Instance);
                    unclassify = unclassify?.MakeGenericMethod(parameter.ParameterType);

                    #region Validation

                    if (unclassify == null)
                        throw new ArgumentNullException(nameof(unclassify));

                    #endregion // Validation

                    var prm = parameter?.Name ?? throw new NullReferenceException();
                    ValueTask<object>? args = (ValueTask<object>?)unclassify.Invoke(
                                                                this, new object[] { arg, prm });
                    arguments[i] = await (args ?? throw new NullReferenceException());

                    #endregion // Unclassify(segmentations)
                }

                #endregion // object[] arguments = ... unclassify

                try
                {
                    await using (Ack.Set(ack))
                    {
                        method.Invoke(instance, arguments);
                    }
                    logger.LogDebug("Consumed event: {0}", meta.Key);
                }
                #region Exception Handling

                catch (OperationCanceledException)
                {
                    ack.Cancel();
                    logger.LogWarning("Canceled event: {0}", meta.Key);
                }
                catch (Exception ex)
                {
                    ack.Cancel();
                    logger.LogError(ex, "event: {0}", meta.Key);
                }

                #endregion // Exception Handling
                finally
                {
                    if (_plan.Options.AckBehavior == AckBehavior.OnSucceed)
                        await ack.AckAsync(); // TODO: check ACK (manual & auto)

                    #region Validation Max Messages Limit

                    if (_maxMessages != 0 && _maxMessages <= count)
                    {
                        await DisposeAsync();
                    }

                    #endregion // Validation Max Messages Limit
                }
            }

            #endregion // ConsumingAsync

            #region Unclassify

            /// <summary>
            /// Unclassify the announcement.
            /// </summary>
            /// <typeparam name="TParam"></typeparam>
            /// <param name="arg">The argument.</param>
            /// <param name="argumentName">Name of the argument.</param>
            /// <returns></returns>
            /// <exception cref="NotSupportedException"></exception>
            protected async ValueTask<object?> UnclassifyAsync<TParam>(Announcement arg, string argumentName)
            {
                foreach (var strategy in _plan.SegmentationStrategies)
                {
                    var (isValid, value) = await strategy.TryUnclassifyAsync<TParam>(arg.Segments, arg.Metadata.Operation, argumentName, _plan.Options);
                    if (isValid)
                        return value;
                }
                throw new NotSupportedException();
            }

            #endregion // Unclassify

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
                    return ValueTaskStatic.CompletedValueTask;

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
