﻿namespace EventSourcing.Backbone.Building
{
    /// <summary>
    /// Bridge segmentation
    /// </summary>
    public class ProducerSegmentationStrategyBridge : IProducerAsyncSegmentationStrategy
    {
        private readonly IProducerSegmentationStrategy _sync;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="sync">The synchronize.</param>
        public ProducerSegmentationStrategyBridge(
            IProducerSegmentationStrategy sync)
        {
            _sync = sync;
        }

        #endregion // Ctor

        #region TryClassifyAsync

        /// <summary>
        /// Try to classifies instance into different segments.
        /// Segments is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// EXPECTED to return the segments argument if it not responsible of
        /// specific parameter handling.
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
        /// the segments argument if don't responsible for segmentation of the type.
        /// </returns>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        ValueTask<Bucket> IProducerAsyncSegmentationStrategy.TryClassifyAsync<T>(
            Bucket segments,
            string operation,
            string argumentName,
            T producedData,
            EventSourceOptions options)
        {
            return _sync.Classify(segments, operation, argumentName, producedData, options).ToValueTask();
        }

        #endregion // TryClassifyAsync
    }
}