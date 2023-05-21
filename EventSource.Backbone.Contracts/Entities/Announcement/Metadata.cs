﻿using System.Diagnostics;

namespace EventSource.Backbone
{    /// <summary>
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
    [DebuggerDisplay("{Operation} [{MessageId}]: Origin:{Origin}, {Environment}->{Partition}->{Shard}, EventKey:{EventKey}")]
    public record Metadata
    {
        #region MessageId

        /// <summary>
        /// The message identifier (channel provider agnostic).
        /// </summary>
        /// <remarks>
        /// New Id will be created on copy scenario (see: Linked)
        /// </remarks>
        public string MessageId { get; init; } = Guid.NewGuid().ToString("N");

        #endregion // MessageId

        #region EventKey

        /// <summary>
        /// The consumer side representation of the event key as represent by a specific channel provider.
        /// </summary>
        public string EventKey { get; init; } = string.Empty;

        #endregion // EventKey

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
        public required string Operation { get; init; } 

        #endregion Operation 

        #region Environment

        /// <summary>
        /// Gets the origin environment of the message.
        /// </summary>
        public string Environment { get; init; } = string.Empty;

        #endregion  Environment

        #region Uri

        /// <summary>
        /// The stream identifier (the URI combined with the environment separate one stream from another)
        /// </summary>
        public required string Uri { get; init; }

        #endregion  Uri

        #region Origin

        /// <summary>
        /// The message origin
        /// </summary>
        public MessageOrigin Origin { get; init; } = MessageOrigin.Original;

        #endregion Origin 

        #region Linked

        /// <summary>
        /// Gets a linked metadata (usually in case of Origin = Copy).
        /// </summary>
        public Metadata? Linked { get; init; }

        #endregion // Linked

        #region ChannelType

        /// <summary>
        /// Gets or sets the shard.
        /// </summary>
        public string ChannelType { get; init; } = string.Empty;

        #endregion ChannelType 

        #region ToString

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"{EventKey}, {this.FullUri()}";

        #endregion // ToString
    }

    /// <summary>
    /// Metadata extensions
    /// </summary>
    public static class MetadataExtensions
    {
        private const string EMPTY_KEY = "~EMPTY~";

        public static readonly Metadata Empty = new Metadata { MessageId = EMPTY_KEY, ChannelType = "NONE", EventKey = string.Empty, Operation="NONE", Uri=string.Empty };

        #region Duration

        /// <summary>
        /// Calculation of Duration since produce time
        /// </summary>
        public static TimeSpan? Duration(this Metadata meta) => DateTimeOffset.Now - meta.ProducedAt;

        #endregion // Duration

        #region FullUri

        /// <summary>
        /// Gets the partition:shard as key.
        /// </summary>
        public static string FullUri(this Metadata meta, char separator = ':')
        {
            if (string.IsNullOrEmpty(meta.Environment))
                return meta.Uri;
            Env env = meta.Environment;
            string envFormatted = env.Format();
            return $"{envFormatted}{separator}{meta.Uri}";
        }

        #endregion // FullUri

        #region IsEmpty

        /// <summary>
        /// Indicate whether it an empty metadata
        /// </summary>
        public static bool IsEmpty(this Metadata meta) => meta.MessageId == EMPTY_KEY;

        #endregion // IsEmpty
    }
}
