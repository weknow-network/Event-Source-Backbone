using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Non-generics form of announcement representation,
    /// used to transfer data via channels.
    /// </summary>
    [DebuggerDisplay("Announcement [{Metadata.MessageId}]: Version = [{Metadata.Version}], DataSegments = [{Metadata.DataSegments}]")]
    public sealed class AnnouncementRaw
    {
        #region Empty

        /// <summary>
        /// The empty
        /// </summary>
        public static readonly AnnouncementRaw Empty = new AnnouncementRaw();

        #endregion // Empty

        #region Ctor

        /// <summary>
        /// Only for serialization support.
        /// </summary>
        private AnnouncementRaw()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnouncementRaw" /> class.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="segments">The segments.</param>
        public AnnouncementRaw(
            AnnouncementMetadata metadata,
            ImmutableDictionary<string, ReadOnlyMemory<byte>> segments)
        {
            _metadata = metadata;
            _segments = segments;
        }

        #endregion // Ctor

        #region Metadata

        private AnnouncementMetadata _metadata = AnnouncementMetadata.Empty;
        /// <summary>
        /// Gets or sets the metadata.
        /// </summary>
        public AnnouncementMetadata Metadata
        {
            get => _metadata;
            [Obsolete("Exposed for the serializer", true)]
            set => _metadata = value;
        }

        #endregion Metadata 

        #region Segments
        
        private ImmutableDictionary<string, ReadOnlyMemory<byte>> _segments = ImmutableDictionary<string, ReadOnlyMemory<byte>>.Empty;
        /// <summary>
        /// Gets or sets the segments.
        /// Segmentation is done at the sending side, 
        /// by Segmentation provider which can be register in order
        /// to segments different parts of the messages.
        /// The motivation of segmentation can come from regulation like
        /// GDPR (right to erasure: https://gdpr-info.eu/).
        /// </summary>
        /// <example>
        /// Segmentation provider can split the message 
        /// into personal and non-personal segments.
        /// </example>
        public ImmutableDictionary<string, ReadOnlyMemory<byte>> Segments
        {
            get => _segments;
            [Obsolete("Exposed for the serializer", true)]
            set => _segments = value;
        }

        #endregion Segments 

        #region InterceptedData

        private ImmutableDictionary<string, ReadOnlyMemory<byte>> _interceptedData = ImmutableDictionary<string, ReadOnlyMemory<byte>>.Empty;
        /// <summary>
        /// Gets or sets the mapping of data which 
        /// was created by interceptors on the send side
        /// and should be evaluate by the interceptor at consume side.
        /// </summary>
        public ImmutableDictionary<string, ReadOnlyMemory<byte>> InterceptedData
        {
            get => _interceptedData;
            [Obsolete("Exposed for the serializer", true)]
            set => _interceptedData = value;
        }

        #endregion InterceptedData 
    }
}
