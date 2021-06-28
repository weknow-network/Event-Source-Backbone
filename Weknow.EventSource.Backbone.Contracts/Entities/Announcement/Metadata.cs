using System;
using System.Diagnostics;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// <![CDATA[Represent metadata of announcement (event).
    /// This information is available via Metadata.Current 
    /// as long as the async-context exists.
    /// Recommended to materialize it before sending to queue and
    /// use Metadata.SetContext(metadata)
    /// </summary>
    /// <remarks>
    /// Unlike the segments, this part can be flow with
    /// message & will be set as async-context.]]> 
    /// </summary>
    [DebuggerDisplay("{Partition}/{Shard} [{MessageId} at {ProducedAt}]")]
    public sealed class Metadata // : IEquatable<Metadata?>
    {
        #region Empty

        /// <summary>
        /// The empty
        /// </summary>
        public static readonly Metadata Empty = new Metadata();

        #endregion // Empty

        #region Ctor

        /// <summary>
        /// Only for serialization support.
        /// </summary>
        private Metadata()
        {
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="partition">The partition.</param>
        /// <param name="shard">The shard.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="producedAt">The produced at.</param>
        public Metadata(
            string messageId,
            string partition,
            string shard,
            string operation,
            DateTimeOffset? producedAt = null)
        {
            MessageId = messageId;
            ProducedAt = producedAt ?? DateTimeOffset.Now;
            Partition = partition;
            Shard = shard;
            Operation = operation;
        }

        #endregion // Ctor

        #region MessageId

        /// <summary>
        /// The message identifier.
        /// </summary>
        public string MessageId { get; init; } = Guid.NewGuid().ToString("N");

        #endregion // MessageId

        #region ProducedAt

        /// <summary>
        /// The sending time.
        /// </summary>
        public DateTimeOffset ProducedAt { get; init; } = DateTimeOffset.Now;

        #endregion // ProducedAt

        #region Operation

        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        public string Operation { get; init; } = string.Empty;

        #endregion Operation 

        #region Duration

        /// <summary>
        /// Calculation of Duration since produce time
        /// </summary>
        public TimeSpan? Duration => DateTimeOffset.Now - ProducedAt;

        #endregion // Duration

        #region Partition

        /// <summary>
        /// Gets or sets the partition.
        /// </summary>
        public string Partition { get; init; } = string.Empty;

        #endregion Partition 

        #region Shard

        /// <summary>
        /// Gets or sets the shard.
        /// </summary>
        public string Shard { get; init; } = string.Empty;

        #endregion Shard 

        #region Key

        /// <summary>
        /// Gets the partition:shard as key.
        /// </summary>
        public string Key => $"{Partition}:{Shard}";

        #endregion // Key
    }
}
