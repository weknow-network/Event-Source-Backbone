using Microsoft.Extensions.Logging;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Base class for the consumer's code generator
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConsumerBase<T>
    {
        private readonly ConsumerPlan _plan;
        private readonly Func<ConsumerMetadata, T> _factory;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="factory">The factory.</param>
        public ConsumerBase(
            ConsumerPlan plan,
            Func<ConsumerMetadata, T> factory)
        {
            _plan = plan;
            _factory = factory;
        }

        #endregion // Ctor

        #region Subscribe

        /// <summary>
        /// Subscribes this instance.
        /// </summary>
        /// <returns></returns>
        public IAsyncDisposable Subscribe()
        {
            var subscription = new Subscription(_plan, _factory);
            return subscription;
        }

        #endregion // Subscribe

        #region class Subscription

        /// <summary>
        /// Represent single consuming subscription
        /// </summary>
        private class Subscription : IAsyncDisposable
        {
            private readonly IConsumerPlan _plan;
            private readonly Func<ConsumerMetadata, T> _factory;
            private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
            private readonly ValueTask _subscriptionLifetime;
            private readonly static ConcurrentDictionary<Subscription, object?> _keepAlive =
                                                new ConcurrentDictionary<Subscription, object?>();

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

                _subscriptionLifetime = channel.SubsribeAsync(
                                                    plan,
                                                    ConsumingAsync,
                                                    plan.Options,
                                                    _cancellation.Token);
            }

            #endregion // Ctor

            #region ConsumingAsync

            /// <summary>
            /// Handles consuming of single event.
            /// </summary>
            /// <param name="arg">The argument.</param>
            /// <param name="ack">
            /// The acknowledge callback which will prevent message from 
            /// being re-fetch from same consumer group.</param>
            /// <returns></returns>
            private async ValueTask ConsumingAsync(Announcement arg, AckCallbackAsync ack)
            {
                var cancellation = _plan.Cancellation;
                var meta = new ConsumerMetadata(arg.Metadata, cancellation);
                var instance = _factory(meta);

                #region Validation

                if (instance == null)
                {
                    var ex = new ArgumentNullException("Cannot create instance");
                    _plan.Logger?.LogWarning(ex, "Consumer fail to create instance");
                    throw ex;
                }

                #endregion // Validation

                // TODO: interception

                var method = instance?.GetType().GetMethod(arg.Metadata.Operation, BindingFlags.Public | BindingFlags.Instance);
                var parameters = method?.GetParameters() ?? throw new ArgumentNullException(); ;
                var arguments = new object[parameters.Length];
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

                try
                {
                    method.Invoke(instance, arguments);
                }
                finally
                {
                    // _plan
                    // TODO: get ack behavior: fire-forget, manual, auto
                    //if (!fireForget && manual)
                    //    ack();
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
                _cancellation.CancelSafe();
                _keepAlive.TryRemove(this, out _);
                return _subscriptionLifetime;
            }

            #endregion // DisposeAsync
        }

        #endregion // class Subscription
    }
}
