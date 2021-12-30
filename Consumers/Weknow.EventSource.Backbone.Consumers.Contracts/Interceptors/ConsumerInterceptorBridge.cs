using System;
using System.Threading.Tasks;


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

        /// <summary>
        /// Interception operation.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="interceptorData">The interceptor data which sets on the
        /// producer stage of the interception.</param>
        /// <returns></returns>
        ValueTask IConsumerAsyncInterceptor.InterceptAsync(Metadata metadata, ReadOnlyMemory<byte> interceptorData)
        {
            _sync.Intercept(metadata, interceptorData);
            return ValueTask.CompletedTask;
        }

        #endregion // InterceptAsync
    }
}