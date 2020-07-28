using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

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
    [DebuggerDisplay("{MessageId}")]
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
            _messageId = messageId;
            _producedAt = producedAt ?? DateTimeOffset.Now;
            _partition = partition;
            _shard = shard;
            _operation = operation;
        }

        #endregion // Ctor

        #region MessageId

        private string _messageId = Guid.NewGuid().ToString("N");

        /// <summary>
        /// The message identifier.
        /// </summary>
        public string MessageId
        {
            get => _messageId;
            [Obsolete("for serialization", true)]
            set => _messageId = value;
        }

        #endregion // MessageId

        #region ProducedAt

        private DateTimeOffset _producedAt = DateTimeOffset.Now;
        /// <summary>
        /// The sending time.
        /// </summary>
        public DateTimeOffset ProducedAt
        {
            get => _producedAt;
            [Obsolete("for serialization", true)]
            set => _producedAt = value;
        }

        #endregion // ProducedAt

        #region Operation

        private string _operation;
        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        public string Operation
        {
            get => _operation;
            [Obsolete("Exposed for the serializer", true)]
            set => _operation = value;
        }

        #endregion Operation 

        #region Duration

        /// <summary>
        /// Calculation of Duration since produce time
        /// </summary>
        public TimeSpan? Duration => DateTimeOffset.Now - ProducedAt;

        #endregion // Duration

        #region Partition

        private string _partition;
        /// <summary>
        /// Gets or sets the partition.
        /// </summary>
        public string Partition
        {
            get => _partition;
            [Obsolete("Exposed for the serializer", true)]
            set => _partition = value;
        }

        #endregion Partition 

        #region Shard

        private string _shard;
        /// <summary>
        /// Gets or sets the shard.
        /// </summary>
        public string Shard
        {
            get => _shard;
            [Obsolete("Exposed for the serializer", true)]
            set => _shard = value;
        }

        #endregion Shard 


        #region // Equality

        public override string ToString()
        {
            throw new NotImplementedException();
        }

        #endregion // Equality
    }
}
