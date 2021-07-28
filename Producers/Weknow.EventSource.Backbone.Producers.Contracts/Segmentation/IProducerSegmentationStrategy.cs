namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
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
    public interface IProducerSegmentationStrategy
    {
        /// <summary>
        /// Classifies instance into different segments.
        /// Segments is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="segments">The segments which was collect so far.
        /// It start as Empty and flow though all the registered segmentation strategies.</param>
        /// <param name="operation">The operation's key which represent the method call at the
        /// producer proxy.
        /// This way you can segment same type into different slot.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="producedData">The produced data.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// bytes for each segment or
        /// Empty if don't responsible for segmentation of the type.
        /// </returns>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        Bucket Classify<T>(
                        Bucket segments,
                        string operation,
                        string argumentName,
                        T producedData,
                        EventSourceOptions options);
    }
}