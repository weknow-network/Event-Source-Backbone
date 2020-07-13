using System;
using System.Collections;
using System.Collections.Immutable;

namespace Weknow.EventSource.Backbone.Building
{

    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    /// <typeparam name="T">The type of the sending data</typeparam>
    public interface IEventSourceProducerDecoratorBuilder<T>:
                                IEventSourceProducerFinalBuilder<T>
                            where T : class
    {
        /// <summary>
        /// Adds segmentation provider.
        /// Responsible of splitting an instance into segments.
        /// Segments is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="segmentationProvider">
        /// A factory which gets:
        /// - options
        /// - forwarding target (in form of an action)
        /// Return implementation of the messaging contract.
        /// </param>
        /// <returns></returns>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        IEventSourceProducerFinalBuilder<T> AddSegmentationProvider(
            Func<EventSourceOptions,
                    Action<string, ReadOnlyMemory<byte>>,
                    T> segmentationProvider);
    }
}
