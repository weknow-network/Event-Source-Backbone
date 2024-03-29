﻿using EventSourcing.Backbone.Building;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IConsumerHooksBuilder
        : IConsumerEnvironmentBuilder,
        IWithCancellation<IConsumerHooksBuilder>
    {
        /// <summary>
        /// Register raw interceptor.
        /// Intercept the consumer side execution before de-serialization.
        /// </summary>
        /// <param name="interceptorData">The interceptor data as the interceptor defined in the producer stage.</param>
        /// <returns></returns>
        IConsumerHooksBuilder RegisterInterceptor(
                                IConsumerInterceptor interceptorData);

        /// <summary>
        /// Register raw interceptor.
        /// Intercept the consumer side execution before de-serialization.
        /// </summary>
        /// <param name="interceptorData">The interceptor data as the interceptor defined in the producer stage.</param>
        /// <returns></returns>
        IConsumerHooksBuilder RegisterInterceptor(
                                IConsumerAsyncInterceptor interceptorData);

        /// <summary>
        /// Responsible of building instance from segmented data.
        /// Segmented data is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="segmentationStrategy">The segmentation strategy.</param>
        /// <returns></returns>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        IConsumerHooksBuilder RegisterSegmentationStrategy(
                                IConsumerSegmentationStrategy segmentationStrategy);
        /// <summary>
        /// Responsible of building instance from segmented data.
        /// Segmented data is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="segmentationStrategy">The segmentation strategy.</param>
        /// <returns></returns>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        IConsumerHooksBuilder RegisterSegmentationStrategy(
                                IConsumerAsyncSegmentationStrategy segmentationStrategy);
    }
}
