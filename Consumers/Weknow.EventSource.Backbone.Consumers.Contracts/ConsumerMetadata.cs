using System.Diagnostics;
using System.Threading;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Represent metadata of message (command / event) metadata of
    /// a communication channel (Pub/Sub, Event Source, REST, GraphQL).
    /// It represent the operation's intent or represent event.
    /// </summary>
    [DebuggerDisplay("{Metadata.Partition}/{Metadata.Shard} [{Metadata.MessageId} at {Metadata.ProducedAt}]")]
    public sealed class ConsumerMetadata 
    {
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
    }
}
