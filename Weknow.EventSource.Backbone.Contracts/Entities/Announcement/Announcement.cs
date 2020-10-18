using System;
using System.Diagnostics;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Non-generics form of announcement representation,
    /// used to transfer data via channels.
    /// </summary>
    [DebuggerDisplay("Announcement [{Metadata.MessageId}]: [{Metadata.Key}]")]
    public sealed class Announcement
    {
        #region Empty

        /// <summary>
        /// The empty
        /// </summary>
        public static readonly Announcement Empty = new Announcement();

        #endregion // Empty

        #region Ctor

        /// <summary>
        /// Only for serialization support.
        /// </summary>
        private Announcement()
        {
        }

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="segments">The segments.</param>
        /// <param name="interceptorData">The interceptor data.</param>
        public Announcement(
            Metadata metadata,
            Bucket segments,
            Bucket interceptorData)
        {
            _metadata = metadata;
            _segments = segments;
            _interceptorsData = interceptorData;
        }

        #endregion // Ctor

        #region Metadata

        private Metadata _metadata = Metadata.Empty;
        /// <summary>
        /// Gets or sets the metadata.
        /// </summary>
        public Metadata Metadata
        {
            get => _metadata;
            [Obsolete("Exposed for the serializer", true)]
            set => _metadata = value;
        }

        #endregion Metadata 

        #region Segments
        
        private Bucket _segments = Bucket.Empty;
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
        public Bucket Segments
        {
            get => _segments;
            [Obsolete("Exposed for the serializer", true)]
            set => _segments = value;
        }

        #endregion Segments 

        #region InterceptorsData
        
        private Bucket _interceptorsData = Bucket.Empty;
        /// <summary>
        /// Interception data (each interceptor responsible of it own data).
        /// Interception can be use for variety of responsibilities like 
        /// flowing auth context or traces, producing metrics, etc.
        /// </summary>
        public Bucket InterceptorsData
        {
            get => _interceptorsData;
            [Obsolete("Exposed for the serializer", true)]
            set => _interceptorsData = value;
        }

        #endregion InterceptorsData 

        #region With

        /// <summary>
        /// <![CDATA[Clone Withes new segments & interceptor-data.]]>
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <param name="interceptorData">The interceptor data.</param>
        /// <returns></returns>
        public Announcement With(
                        Bucket segments,
                        Bucket interceptorData)
        {
            return new Announcement(_metadata, segments, interceptorData);
        }

        #endregion // With
    }
}
