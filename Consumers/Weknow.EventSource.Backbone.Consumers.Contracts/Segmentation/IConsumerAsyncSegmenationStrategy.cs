using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Segments = System.Collections.Immutable.ImmutableDictionary<string, System.ReadOnlyMemory<byte>>;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Responsible of building instance from segmented data.
    /// Segmented data is how the producer sending its raw data to 
    /// the consumer. It's in a form of dictionary when 
    /// keys represent the different segments 
    /// and the value represent serialized form of the segment's data.
    /// </summary>
    /// <example>
    /// Examples for segments can be driven from regulation like
    /// GDPR (personal, non-personal data),
    /// Technical vs Business aspects, etc.
    /// </example>
    public interface IConsumerAsyncSegmentationStrategy
    {
        /// <summary>
        /// Unclassify segmented data into an instance.
        /// Segments is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="segments">The segments which was collect so far.
        /// It start as Empty and flow though all the registered segmentation strategies.</param>
        /// <param name="operation">The operation's key which represent the method call at the
        /// producer proxy.
        /// This way you can segment same type into different slot.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// Materialization of the segments.
        /// </returns>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        ValueTask<(bool, T)> TryUnclassifyAsync<T>(
                Segments segments,
                string operation,
                string argumentName,
                EventSourceOptions options);
    }
}