using System;
using System.Collections.Immutable;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Responsible of building instance from segmented data.
    /// Segmented data is how the producer sending its raw data to 
    /// the consumer. It's in a form of dictionary when 
    /// keys represent the different segments 
    /// and the value represent serialized form of the segment's data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <example>
    /// Examples for segments can be driven from regulation like
    /// GDPR (personal, non-personal data),
    /// Technical vs Business aspects, etc.
    /// </example>
    public interface IConsumerSegmenationProvider<T> where T : notnull
    {
        /// <summary>
        /// Unclassify segmented data into an instance.
        /// Segments is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="segments">Segments form of the original message.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns></returns>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        T Unclassify(
                ImmutableDictionary<string, ReadOnlyMemory<byte>> segments,
                IDataSerializer serializer);
    }
}