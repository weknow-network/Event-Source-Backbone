using System;
using System.Threading.Tasks;

using Segments = System.Collections.Immutable.ImmutableDictionary<string, System.ReadOnlyMemory<byte>>;

namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IEventSourceProducerHooksBuilder:
        IEventSourceProducerSpecializeBuilder
    {
        #region AddInterceptor

        /// <summary>
        /// Adds Producer interceptor (stage = after serialization).
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns></returns>
        IEventSourceProducerHooksBuilder AddInterceptor(
                                IProducerRawInterceptor interceptor);

        /// <summary>
        /// Adds Producer interceptor (stage = after serialization).
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns></returns>
        IEventSourceProducerHooksBuilder AddInterceptor(
                                IProducerRawAsyncInterceptor interceptor);

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
        /// <param name="segmenationStrategy">A strategy of segmentation.</param>
        /// <returns></returns>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        IEventSourceProducerHooksBuilder UseSegmentation(
                                IProducerAsyncSegmenationStrategy segmenationStrategy);
        /// <summary>
        /// Register segmentation strategy,
        /// Segmentation responsible of splitting an instance into segments.
        /// Segments is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="segmenationStrategy">A strategy of segmentation.</param>
        /// <returns></returns>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        IEventSourceProducerHooksBuilder UseSegmentation(
                                IProducerSegmenationStrategy segmenationStrategy);
        /// <summary>
        /// Register segmentation strategy,
        /// Segmentation responsible of splitting an instance into segments.
        /// Segments is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="segmenationStrategy">
        /// A strategy of segmentation.
        /// Gets the current mapped segments, operation key, payload, configuration
        /// and return segments.
        /// </param>
        /// <returns></returns>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        IEventSourceProducerHooksBuilder UseSegmentation<T>(
                                Func<Segments, string, T, EventSourceOptions, Segments> segmenationStrategy);
        /// <summary>
        /// Register segmentation strategy,
        /// Segmentation responsible of splitting an instance into segments.
        /// Segments is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="segmenationStrategy">
        /// A strategy of segmentation.
        /// Gets the current mapped segments, operation key, payload, configuration
        /// and return segments.
        /// </param>
        /// <returns></returns>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        IEventSourceProducerHooksBuilder UseSegmentation<T>(
                                Func<Segments, string, T, EventSourceOptions, ValueTask<Segments>> segmenationStrategy);

        #endregion // UseSegmentation
    }
}
