using System.Diagnostics;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Represent metadata of message (command / event) metadata of
    /// a communication channel (Pub/Sub, Event Source, REST, GraphQL).
    /// It represent the operation's intent or represent event.
    /// </summary>
    [DebuggerDisplay("{Metadata.Uri} [{Metadata.MessageId} at {Metadata.ProducedAt}]")]
    public sealed class ConsumerMetadata : IAckOperations
    {
        internal static readonly AsyncLocal<ConsumerMetadata> _metaContext = new AsyncLocal<ConsumerMetadata>();

        /// <summary>
        /// Get the metadata context
        /// </summary>
        public static ConsumerMetadata Context => _metaContext.Value ?? throw new EventSourcingException(
                        """
                        Consumer metadata doesn't available on the current context 
                        (make sure you try to consume it within a scope of a consuming method call)");
                        """);

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="consumingCancellation">The consuming cancellation
        /// (stop consuming call-back on cancellation).</param>
        public ConsumerMetadata(
            Metadata metadata,
            CancellationToken consumingCancellation)
        {
            Metadata = metadata;
            ConsumingCancellation = consumingCancellation;
        }

        #endregion // Ctor

        #region Metadata

        /// <summary>
        /// Gets the metadata.
        /// </summary>
        public Metadata Metadata { get; }

        #endregion // Metadata

        #region ConsumingCancellation

        /// <summary>
        /// Cancel the consuming process.
        /// </summary>
        public CancellationToken ConsumingCancellation { get; }

        #endregion // ConsumingCancellation

        #region Cast overloads

        /// <summary>
        /// Performs an implicit conversion.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Metadata(ConsumerMetadata? instance)
        {
            return instance?.Metadata ?? MetadataExtensions.Empty;
        }

        #endregion // Cast overloads

        #region AckAsync

        /// <summary>
        /// Preform acknowledge (which should prevent the 
        /// message from process again by the consumer)
        /// Must be execute from a consuming scope (i.e. method call invoked by the consumer's event processing).
        /// </summary>
        public ValueTask AckAsync() => Ack.Current.AckAsync();

        #endregion // AckAsync

        #region AckAsync

        /// <summary>
        /// Cancel acknowledge (will happen on error in order to avoid ack on succeed)
        /// </summary>
        /// Must be execute from a consuming scope (i.e. method call invoked by the consumer's event processing).
        /// <returns></returns>
        public ValueTask CancelAsync() => Ack.Current.AckAsync();

        #endregion // AckAsync

    }
}
