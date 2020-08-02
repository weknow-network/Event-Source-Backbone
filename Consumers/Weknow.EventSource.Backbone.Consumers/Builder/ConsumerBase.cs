using System;
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
        private class Subscription: IAsyncDisposable
        {
            private readonly ConsumerPlan _plan;
            private readonly Func<ConsumerMetadata, T> _factory;
            private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
            private readonly ValueTask _subscriptionLifetime;
            private readonly GCHandle? _gcHandle;

            #region Ctor

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="plan">The plan.</param>
            /// <param name="factory">The factory.</param>
            public Subscription(
                ConsumerPlan plan,
                Func<ConsumerMetadata, T> factory)
            {
                _plan = plan;
                var channel = _plan.Channel;
                _factory = factory;
                // TODO: [bnaya, 2020-07] Review with Avi
                if(plan.Options.KeepAlive)
                    _gcHandle = GCHandle.Alloc(this, GCHandleType.Normal);

                _subscriptionLifetime = channel.ReceiveAsync(
                                                    ConsumingAsync,
                                                    _plan.Options, 
                                                    _cancellation.Token);
            }

            #endregion // Ctor

            #region ConsumingAsync

            /// <summary>
            /// Handles consuming of single event.
            /// </summary>
            /// <param name="arg">The argument.</param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">
            /// Cannot cre
            /// or
            /// unclassify
            /// </exception>
            private async ValueTask ConsumingAsync(Announcement arg)
            {
                var cancellations = CancellationToken.None;
                if (_plan.Cancellations.Count != 0)
                {
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(
                                        _plan.Cancellations.ToArray());
                    cancellations = cts.Token;
                }
                var meta = new ConsumerMetadata(arg.Metadata, cancellations);
                var instance = _factory(meta);

                #region Validation

                if (instance == null)
                {
                    // TODO: Log warning $"{nameof(strategy.TryClassifyAsync)} don't expect to return null value");
                    throw new ArgumentNullException("Cannot cre");
                }

                #endregion // Validation

                var method = instance?.GetType().GetMethod(arg.Metadata.Operation, BindingFlags.Public | BindingFlags.Instance);
                var parameters = method?.GetParameters() ?? throw new ArgumentNullException(); ;
                var arguments = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    ParameterInfo? parameter = parameters[i];
                    MethodInfo? unclassify = this.GetType().GetMethod("Unclassify", BindingFlags.NonPublic | BindingFlags.Instance);
                    unclassify = unclassify?.MakeGenericMethod(parameter.ParameterType);

                    #region Validation

                    if (unclassify == null)
                        throw new ArgumentNullException(nameof(unclassify));

                    #endregion // Validation

                    var prm = parameter?.Name ?? throw new NullReferenceException();
                    ValueTask<object>? args = (ValueTask<object>?)unclassify.Invoke(
                                                                this, new object[] { arg, prm });
                    
                    arguments[i] = await (args ?? throw new NullReferenceException());
                }
                method.Invoke(instance, arguments);
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
            private async ValueTask<object?> UnclassifyAsync<TParam>(Announcement arg, string argumentName)
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
                _gcHandle?.Free();
                return _subscriptionLifetime;
            }

            #endregion // DisposeAsync
        }

        #endregion // class Subscription
    }
}
