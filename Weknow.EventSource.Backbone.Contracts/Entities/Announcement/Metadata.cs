using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Represent metadata of announcement (event).
    /// This information is available via Metadata.Current 
    /// as long as the async-context exists.
    /// Recommended to materialize it before sending to queue and
    /// use Metadata.SetContext(metadata)
    /// </summary>
    /// <remarks>
    /// Unlike the segments, this part can be flow with
    /// message & will be set as async-context.
    /// </remarks>
    [DebuggerDisplay("{MessageId}")]
    public sealed class Metadata // : IEquatable<Metadata?>
    {
        #region Empty

        /// <summary>
        /// The empty
        /// </summary>
        public static readonly Metadata Empty = new Metadata();

        #endregion // Empty

        private static AsyncLocal<Metadata?> AsyncContext = new AsyncLocal<Metadata?>();
        
        public static Metadata? Current => AsyncContext.Value;
        public static void SetContext(Metadata metadata) =>
                            AsyncContext.Value = metadata;

        #region Ctor

        /// <summary>
        /// Only for serialization support.
        /// </summary>
        private Metadata()
        {
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

        #region Retries

        private ushort _retries;
        /// <summary>
        /// Gets the retries time of re-consuming the message.
        /// </summary>
        public ushort Retries
        {
            get => _retries;
            [Obsolete("Exposed for the serializer", true)]
            set => _retries = value;
        }

        #endregion Retries 

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

        #region Duration

        /// <summary>
        /// Calculation of Duration since produce time
        /// </summary>
        public TimeSpan? Duration => DateTimeOffset.Now - ProducedAt;

        #endregion // Duration

        #region // Equality

        public override string ToString()
        {
            throw new NotImplementedException();
        }

        #endregion // Equality
    }
}
