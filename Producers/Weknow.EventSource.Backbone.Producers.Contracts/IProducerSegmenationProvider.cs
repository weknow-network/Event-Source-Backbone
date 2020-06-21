using System;
using System.Collections.Immutable;

namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Responsible of splitting an instance into segments.
    /// Segments is how the producer sending its raw data to 
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
    public interface IProducerSegmenationProvider<T> 
                                    where T : notnull
    {
        /// <summary>
        /// Classifies instance into different segments.
        /// Segments is how the producer sending its raw data to 
        /// the consumer. It's in a form of dictionary when 
        /// keys represent the different segments 
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="producedData"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        ImmutableDictionary<string, ReadOnlyMemory<byte>> Classify(
            T producedData, 
            IDataSerializer serializer);
    }
}