using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Weknow.EventSource.Backbone.Building
{

    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    /// <typeparam name="T">The type of the sending data</typeparam>
    public interface IEventSourceProducer3Builder<T>:
                                IEventSourceProducer4Builder<T>
                            where T : notnull
    {
        /// <summary>
        /// Adds interceptor for the serialized-type.
        /// </summary>
        /// <param name="intercept">The intercept.</param>
        /// <returns></returns>
        IEventSourceProducer3Builder<T> AddInterceptor(
                    IProducerInterceptor<T> intercept);

        /// <summary>
        /// Adds interceptor for the serialized-type.
        /// </summary>
        /// <param name="intercept">The intercept.</param>
        /// <returns></returns>
        IEventSourceProducer3Builder<T> AddAsyncInterceptor(
                    IProducerAsyncInterceptor<T> intercept);

        /// <summary>
        /// Adds segmentation provider.
        /// Responsible of splitting an instance into segments.
        /// Segments is how the producer sending its raw data to 
        /// the consumer. It's in a form of dictionary when 
        /// keys represent the different segments 
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        /// <returns></returns>
        IEventSourceProducer4Builder<T> AddSegmentationProvider(
            IProducerSegmenationProvider<T> segmentationProvider);

        /// <summary>
        /// Adds segmentation provider.
        /// Responsible of splitting an instance into segments.
        /// Segments is how the producer sending its raw data to 
        /// the consumer. It's in a form of dictionary when 
        /// keys represent the different segments 
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        /// <returns></returns>
        IEventSourceProducer4Builder<T> AddSegmentationProvider(
            Func<T, 
                IDataSerializer,
                ImmutableDictionary<string, ReadOnlyMemory<byte>>> segmentationProvider);
    }
}
