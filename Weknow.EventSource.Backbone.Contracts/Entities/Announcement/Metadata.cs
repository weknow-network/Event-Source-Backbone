﻿using System;
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

        #region EventKey

        /// <summary>
        /// The consumer side representation of the event key as represent by a specific channel.
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
        public string Operation { get; init; } = string.Empty;

        #endregion Operation 

        #region Environment

        /// <summary>
        /// Gets the origin environment of the message.
        /// </summary>
        public string Environment { get; init; } = string.Empty;

        #endregion  Environment

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
        private const string EMPTY_KEY = "~EMPTY~";

        public static readonly Metadata Empty = new Metadata { MessageId = EMPTY_KEY };

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
        public static string Key(this Metadata meta, char separator = ':')
        {
            if (string.IsNullOrEmpty(meta.Environment))
                return $"{meta.Partition}{separator}{meta.Shard}";
            return $"{meta.Environment}{separator}{meta.Partition}{separator}{meta.Shard}";
        }

        #endregion // Key

        #region IsEmpty

        /// <summary>
        /// Indicate whether it an empty metadata
        /// </summary>
        public static bool IsEmpty(this Metadata meta) => meta.MessageId == EMPTY_KEY;

        #endregion // IsEmpty
    }
}
