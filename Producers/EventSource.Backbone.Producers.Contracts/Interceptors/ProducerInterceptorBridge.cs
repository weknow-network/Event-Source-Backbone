﻿namespace EventSource.Backbone.Building
{
    /// <summary>
    /// Bridge segmentation
    /// </summary>
    public class ProducerInterceptorBridge : IProducerAsyncInterceptor
    {
        private readonly IProducerInterceptor _sync;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="sync">The synchronize.</param>
        public ProducerInterceptorBridge(
            IProducerInterceptor sync)
        {
            _sync = sync;
        }

        #endregion // Ctor

        #region InterceptorName

        /// <summary>
        /// Unique name which represent the correlation
        /// between the producer and consumer interceptor.
        /// It's recommended to use URL format.
        /// </summary>
        string IInterceptorName.InterceptorName => _sync.InterceptorName;

        #endregion // InterceptorName

        #region InterceptAsync

        /// <summary>
        /// Interception operation.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="segments">The segments.</param>
        /// <returns>
        /// Data which will be available to the
        /// consumer stage of the interception.
        /// </returns>
        ValueTask<ReadOnlyMemory<byte>> IProducerAsyncInterceptor.InterceptAsync(
                                    Metadata metadata,
                                    Bucket segments)
        {
            var result = _sync.Intercept(metadata);
            return result.ToValueTask();
        }

        #endregion // InterceptAsync
    }
}