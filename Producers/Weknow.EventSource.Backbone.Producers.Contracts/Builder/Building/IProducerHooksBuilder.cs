namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IProducerHooksBuilder :
        IProducerLoggerBuilder
    {
        #region Parameters

        /// <summary>
        /// Gets the producer's plan.
        /// </summary>
        ProducerPlan Plan { get; }

        #endregion // Parameters

        #region AddInterceptor

        /// <summary>
        /// Adds Producer interceptor (stage = after serialization).
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns></returns>
        IProducerHooksBuilder AddInterceptor(
                                IProducerInterceptor interceptor);

        /// <summary>
        /// Adds Producer interceptor (Timing: after serialization).
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns></returns>
        IProducerHooksBuilder AddInterceptor(
                                IProducerAsyncInterceptor interceptor);

        #endregion // AddInterceptor

        #region UseSegmentation

        /// <summary>
        /// Register segmentation strategy,
        /// Segmentation responsible of splitting an instance into segments.
        /// Segments is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="segmentationStrategy">A strategy of segmentation.</param>
        /// <returns></returns>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        IProducerHooksBuilder UseSegmentation(
                                IProducerAsyncSegmentationStrategy segmentationStrategy);
        /// <summary>
        /// Register segmentation strategy,
        /// Segmentation responsible of splitting an instance into segments.
        /// Segments is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="segmentationStrategy">A strategy of segmentation.</param>
        /// <returns></returns>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        IProducerHooksBuilder UseSegmentation(
                                IProducerSegmentationStrategy segmentationStrategy);

        ///// <summary>
        ///// Register segmentation strategy,
        ///// Segmentation responsible of splitting an instance into segments.
        ///// Segments is how the producer sending its raw data to
        ///// the consumer. It's in a form of dictionary when
        ///// keys represent the different segments
        ///// and the value represent serialized form of the segment's data.
        ///// </summary>
        ///// <param name="segmentationStrategy">
        ///// A strategy of segmentation.
        ///// Gets the current mapped segments, operation key, payload, configuration
        ///// and return segments.
        ///// </param>
        ///// <returns></returns>
        ///// <example>
        ///// Examples for segments can be driven from regulation like
        ///// GDPR (personal, non-personal data),
        ///// Technical vs Business aspects, etc.
        ///// </example>
        //IEventSourceProducerHooksBuilder UseSegmentation<T>(
        //                        Func<Segments, string, T, EventSourceOptions, Segments> segmentationStrategy);
        ///// <summary>
        ///// Register segmentation strategy,
        ///// Segmentation responsible of splitting an instance into segments.
        ///// Segments is how the producer sending its raw data to
        ///// the consumer. It's in a form of dictionary when
        ///// keys represent the different segments
        ///// and the value represent serialized form of the segment's data.
        ///// </summary>
        ///// <param name="segmentationStrategy">
        ///// A strategy of segmentation.
        ///// Gets the current mapped segments, operation key, payload, configuration
        ///// and return segments.
        ///// </param>
        ///// <returns></returns>
        ///// <example>
        ///// Examples for segments can be driven from regulation like
        ///// GDPR (personal, non-personal data),
        ///// Technical vs Business aspects, etc.
        ///// </example>
        //IEventSourceProducerHooksBuilder UseSegmentation<T>(
        //                        Func<Segments, string, T, EventSourceOptions, ValueTask<Segments>> segmentationStrategy);

        #endregion // UseSegmentation
    }
}
