using System.Diagnostics;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Represent metadata of message (command / event) metadata of
    /// a communication channel (Pub/Sub, Event Source, REST, GraphQL).
    /// It represent the operation's intent or represent event.
    /// </summary>
    [DebuggerDisplay("{Metadata.Uri} [{Metadata.MessageId} at {Metadata.ProducedAt}]")]
    public sealed class ConsumerContext : IAckOperations
    {
        internal static readonly AsyncLocal<ConsumerContext> _metaContext = new AsyncLocal<ConsumerContext>();

        /// <summary>
        /// Get the metadata context
        /// </summary>
        public static ConsumerContext Context => _metaContext.Value ?? throw new EventSourcingException(
                        """
                        Consumer metadata doesn't available on the current context 
                        (make sure you try to consume it within a scope of a consuming method call)");
                        """);

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="options">The consumer execution's options.</param>
        /// <param name="consumingCancellation">The consuming cancellation
        /// (stop consuming call-back on cancellation).</param>
        public ConsumerContext(
            Metadata metadata,
            ConsumerOptions options,
            CancellationToken consumingCancellation)
        {
            Metadata = metadata;
            Options = options;
            ConsumingCancellation = consumingCancellation;
        }

        #endregion // Ctor

        #region Metadata

        /// <summary>
        /// Gets the metadata.
        /// </summary>
        public Metadata Metadata { get; }

        #endregion // Metadata

        #region Options

        /// <summary>
        /// Gets the options.
        /// </summary>
        public ConsumerOptions Options { get; }

        #endregion // Options

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
        public static implicit operator Metadata(ConsumerContext? instance)
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
        /// <param name="cause">The cause of the acknowledge.</param>
        /// <returns></returns>
        public ValueTask AckAsync(AckBehavior cause = AckBehavior.Manual) => Ack.Current.AckAsync(cause);

        #endregion // AckAsync

        #region CancelAsync

        /// <summary>
        /// Cancel acknowledge (will happen on error in order to avoid ack on succeed)
        /// </summary>
        /// <param name="cause">The cause of the cancellation.</param>
        /// <returns></returns>
        /// Must be execute from a consuming scope (i.e. method call invoked by the consumer's event processing).
        public ValueTask CancelAsync(AckBehavior cause = AckBehavior.Manual) => Ack.Current.CancelAsync(cause);

        #endregion // CancelAsync
    }
}
