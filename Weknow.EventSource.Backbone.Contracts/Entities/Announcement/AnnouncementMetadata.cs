using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Represent metadata of message (command / event) metadata of
    /// a communication channel (Pub/Sub, Event Source, REST, GraphQL).
    /// It represent the operation's intent or represent event.
    /// </summary>
    [DebuggerDisplay("{Intent} [{MessageId}]: DataType= {DataType}")]
    public sealed class AnnouncementMetadata : IEquatable<AnnouncementMetadata?>
    {
        #region Empty

        /// <summary>
        /// The empty
        /// </summary>
        public static readonly AnnouncementMetadata Empty = new AnnouncementMetadata();

        #endregion // Empty

        #region Ctor

        /// <summary>
        /// Only for serialization support.
        /// </summary>
        private AnnouncementMetadata()
        {
        }

        /// <summary>
        /// Initializes a new instance        /// </summary>
        /// <param name="intent">Represent logical intention of command/notification.
        /// Either represent intention for action (command)
        /// or type of occurrence (notification)</param>
        /// <param name="channel">The communication channel used to handle this event.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="segmentedBy">The segmented by.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="dispatchTime">The dispatch time.</param>
        /// <param name="duration">The duration.</param>
        public AnnouncementMetadata(
            string intent,
            string channel,
            string dataType,
            string segmentedBy,
            string? messageId = null,
            DateTimeOffset? dispatchTime = null,
            TimeSpan? duration = null)
        {
            _messageId = messageId ?? Guid.NewGuid().ToString("N");
            _dispatchTime = dispatchTime ?? DateTimeOffset.Now;
            _duration = duration;
            _intent = intent;
            _channel = channel;
            _dataType = dataType;
            _segmentedBy = segmentedBy;
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

        #region Flags

        private EventFlags _flags;
        /// <summary>
        /// Gets or sets
        /// Represent hints for event execution.
        /// Some execution content (like reconstruction) might like to ignore
        /// event with specific flags.
        /// </summary>
        public EventFlags Flags
        {
            get => _flags;
            [Obsolete("Exposed for the serializer", true)]
            set => _flags = value;
        }

        #endregion Flags 

        #region Intent

        private string _intent = string.Empty;
        /// <summary>        
        /// Represent logical intention of command/notification.
        /// Either represent intention for action (command)
        /// or type of occurrence (notification).
        /// </summary>
        public string Intent
        {
            get => _intent;
            [Obsolete("for serialization", true)]
            set => _intent = value;
        }

        #endregion // Intent

        #region DataType

        private string _dataType = string.Empty;
        /// <summary>        
        /// The type of the announcement's data.
        /// </summary>
        public string DataType
        {
            get => _dataType;
            [Obsolete("for serialization", true)]
            set => _dataType = value;
        }

        #endregion // DataType

        #region SegmentedBy

        private string _segmentedBy = string.Empty;
        /// <summary>        
        /// Indicate the segment provider used for the announcement segmentation.
        /// </summary>
        public string SegmentedBy
        {
            get => _segmentedBy;
            [Obsolete("for serialization", true)]
            set => _segmentedBy = value;
        }

        #endregion // SegmentedBy

        #region DespatchTime

        private DateTimeOffset _dispatchTime = DateTimeOffset.Now;
        /// <summary>
        /// The sending time.
        /// </summary>
        public DateTimeOffset DispatchTime
        {
            get => _dispatchTime;
            [Obsolete("for serialization", true)]
            set => _dispatchTime = value;
        }

        #endregion // DespatchTime

        #region Duration

        private TimeSpan? _duration;
        /// <summary>
        /// Time pass between sending to consuming of the message.
        /// </summary>
        public TimeSpan? Duration
        {
            get => _duration;
            [Obsolete("for serialization", true)]
            set => _duration = value;
        }

        #endregion // Duration

        #region Channel

        private string _channel = string.Empty;
        /// <summary>
        /// Represent the communication channel which the message came from.
        /// </summary>
        public string Channel
        {
            get => _channel;
            [Obsolete("for serialization", true)]
            set => _channel = value;
        }

        #endregion // Channel

        #region // Deconstruct

        ///// <summary>
        ///// Enable the de-construct functionality.
        ///// </summary>
        ///// <param name="messageId">The message identifier.</param>
        ///// <param name="version">The version.</param>
        ///// <param name="dataSegments">The data segments.</param>
        ///// <param name="dispatchTime">The dispatch time.</param>
        //public void Deconstruct(
        //    out string messageId,
        //    out string version,
        //    out DateTimeOffset dispatchTime)
        //{
        //    messageId = _messageId;
        //    version = _intent;
        //    dispatchTime = _dispatchTime;
        //}

        #endregion // Deconstruct

        #region Equality


        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        ///   <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as AnnouncementMetadata);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(AnnouncementMetadata? other)
        {
            return other != null &&
                   _messageId == other._messageId &&
                   _flags == other._flags &&
                   _intent == other._intent &&
                   _dataType == other._dataType &&
                   _segmentedBy == other._segmentedBy &&
                   _dispatchTime.Equals(other._dispatchTime) &&
                   EqualityComparer<TimeSpan?>.Default.Equals(_duration, other._duration) &&
                   _channel == other._channel;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(_messageId, _flags, _intent, _dataType, _segmentedBy, _dispatchTime, _duration, _channel);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(AnnouncementMetadata? left, AnnouncementMetadata? right)
        {
            return EqualityComparer<AnnouncementMetadata>.Default.Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(AnnouncementMetadata? left, AnnouncementMetadata? right)
        {
            return !(left == right);
        }

        #endregion // Equality
    }
}
