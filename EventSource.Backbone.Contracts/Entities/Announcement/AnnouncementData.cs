﻿using System.Diagnostics;

namespace EventSource.Backbone
{
    /// <summary>
    /// Non-generics form of announcement representation,
    /// used to transfer data via channels.
    /// </summary>
    [DebuggerDisplay("{Operation} [{MessageId}]: Origin:{Origin}, {Environment}->{Partition}->{Shard}, EventKey:{EventKey}")]
    public record AnnouncementData : Metadata
    {
        #region Data

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
        public Bucket Data { get; init; } = Bucket.Empty;

        #endregion Data 

        #region ExtractMeta

        /// <summary>
        /// Extracts the meta.
        /// </summary>
        /// <returns></returns>
        public Metadata ExtractMeta() => new Metadata
        {
            ChannelType = this.ChannelType,
            Environment = this.Environment,
            EventKey = this.EventKey,
            MessageId = this.MessageId,
            Operation = this.Operation,
            Uri = this.Uri,
            ProducedAt = this.ProducedAt,
        };

        #endregion // ExtractMeta

        #region Cast overloads

        /// <summary>
        /// Performs an implicit conversion.
        /// </summary>
        /// <param name="announcement">The announcement.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator AnnouncementData(Announcement announcement)
        {
            var meta = announcement.Metadata;

            return new AnnouncementData
            {
                Data = announcement.Segments,
                ChannelType = meta.ChannelType,
                Environment = meta.Environment,
                EventKey = meta.EventKey,
                MessageId = meta.MessageId,
                ProducedAt = meta.ProducedAt,
                Operation = meta.Operation,
                Uri = meta.Uri,
            };
        }

        #endregion // Cast overloads
    }
}
