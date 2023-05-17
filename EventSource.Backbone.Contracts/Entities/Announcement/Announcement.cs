using System.Diagnostics;

namespace EventSource.Backbone
{
    /// <summary>
    /// Non-generics form of announcement representation,
    /// used to transfer data via channels.
    /// </summary>
    [DebuggerDisplay("{Metadata}")]
    public record Announcement
    {
        #region Metadata

        /// <summary>
        /// Gets or sets the metadata.
        /// </summary>
        public Metadata Metadata { get; init; } = MetadataExtensions.Empty;

        #endregion Metadata 

        #region Segments

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
        public Bucket Segments { get; init; } = Bucket.Empty;

        #endregion Segments 

        #region InterceptorsData

        /// <summary>
        /// Interception data (each interceptor responsible of it own data).
        /// Interception can be use for variety of responsibilities like 
        /// flowing auth context or traces, producing metrics, etc.
        /// </summary>
        public Bucket InterceptorsData { get; init; } = Bucket.Empty;

        #endregion InterceptorsData 
    }
}