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
    public record Metadata
    {
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

        #region ChannelType

        /// <summary>
        /// Gets or sets the shard.
        /// </summary>
        public string ChannelType { get; init; } = string.Empty;

        #endregion ChannelType 
    }

    /// <summary>
    /// Metadata extensions
    /// </summary>
    public static class MetadataExtensions
    {
        public static readonly Metadata Empty = new Metadata();

        #region Duration

        /// <summary>
        /// Calculation of Duration since produce time
        /// </summary>
        public static TimeSpan? Duration(this Metadata meta) => DateTimeOffset.Now - meta.ProducedAt;

        #endregion // Duration

        #region Key

        /// <summary>
        /// Gets the partition:shard as key.
        /// </summary>
        public static string Key(this Metadata meta) => $"{meta.Partition}:{meta.Shard}";

        #endregion // Key
    }
}
