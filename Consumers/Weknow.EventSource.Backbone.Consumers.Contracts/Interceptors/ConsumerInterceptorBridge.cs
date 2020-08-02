using System;
using System.Threading.Tasks;

using static System.Threading.Tasks.ValueTaskStatic;


namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Bridge segmentation
    /// </summary>
    public class ConsumerInterceptorBridge : IConsumerAsyncInterceptor
    {
        private readonly IConsumerInterceptor _sync;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="sync">The synchronize.</param>
        public ConsumerInterceptorBridge(
            IConsumerInterceptor sync)
        {
            _sync = sync;
        }

        #endregion // Ctor

        #region InterceptorName

        /// <summary>
        /// Unique name which represent the correlation
        /// between the consumer and consumer interceptor.
        /// It's recommended to use URL format.
        /// </summary>
        string IInterceptorName.InterceptorName => _sync.InterceptorName;

        #endregion // InterceptorName

        #region InterceptAsync

        ValueTask IConsumerAsyncInterceptor.InterceptAsync(Metadata metadata, ReadOnlyMemory<byte> interceptorData)
        {
            _sync.Intercept(metadata, interceptorData);
            return CompletedValueTask;
        }

        #endregion // InterceptAsync
    }
}