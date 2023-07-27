using System.Diagnostics;

namespace EventSourcing.Backbone
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
    [DebuggerDisplay("{Operation} [{MessageId}]: Origin={Origin}, Target={Environment}={Uri}, EventKey={EventKey}, StorageTypes={StorageTypes}")]
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
        public Env Environment { get; init; } = string.Empty;

        #endregion  Environment

        #region Uri

        private string _uri = string.Empty;
        /// <summary>
        /// The stream identifier (the URI combined with the environment separate one stream from another)
        /// </summary>
        public required string Uri
        {
            get => _uri;
            init
            {
                _uri = value;
                UriDash = value.ToDash();
            }
        }

        #endregion  Uri

        #region UriDash

        /// <summary>
        /// Gets a lower-case URI with dash separator.
        /// </summary>
        public string UriDash { get; private set; } = string.Empty;

        #endregion // UriDash

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

        #region StorageTypes

        /// <summary>
        /// Gets the storage types which having values.
        /// By setting it into the metadata at the producer side, we reduce IO by ignoring fetching an empty buckets.
        /// </summary>
        public EventBucketCategories StorageTypes { get; init; } = EventBucketCategories.None;

        #endregion // StorageTypes

        #region ParamsSignature

        /// <summary>
        /// Gets the signature of the method's parameters.
        /// </summary>
        public required string ParamsSignature { get; init; }

        #endregion // ParamsSignature

        #region Version

        /// <summary>
        /// Gets the version of the message (as defined in the interface).
        /// </summary>
        public required int Version { get; init; }

        #endregion // Version

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

        public static readonly Metadata Empty = new Metadata { MessageId = EMPTY_KEY, Version = -1, ChannelType = "NONE", EventKey = string.Empty, Operation = "NONE", Uri = string.Empty, ParamsSignature = string.Empty };

        #region Duration

        /// <summary>
        /// Calculation of Duration since produce time
        /// </summary>
        public static TimeSpan? Duration(this Metadata meta) => DateTimeOffset.Now - meta.ProducedAt;

        #endregion // Duration

        #region FullUri

        /// <summary>
        /// Gets the env:URI as key.
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
