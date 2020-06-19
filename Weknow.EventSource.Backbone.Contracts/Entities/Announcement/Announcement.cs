using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{

    /// <summary>
    /// Represent command or occurrence.
    /// Which is ready to consume.
    /// </summary>
    [DebuggerDisplay("Dispatch Info: {Meta.MessageId}")]
    public class Announcement<T>
        where T: notnull
    {
        #region Ctor

        /// <summary>
        /// Only for serialization.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        private Announcement()
        {
        }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageIntent{T}" /> class.
        /// </summary>
        /// <param name="meta">The metadata.</param>
        /// <param name="personalData">The personal data.</param>
        public Announcement(
            AnnouncementMetadata meta,
            T personalData)
        {
            _meta = meta;
            _data = personalData;
        }

        #endregion // Ctor

        #region Meta

        private AnnouncementMetadata _meta = AnnouncementMetadata.Empty;

        /// <summary>
        /// Gets the metadata.
        /// </summary>
        public AnnouncementMetadata Meta
        {
            get { return _meta; }
            [Obsolete("for serialization", true)]
            set { _meta = value; }
        }

        #endregion // Meta

        #region Data

        private T _data;
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public T Data
        {
            get => _data;
            [Obsolete("Exposed for the serializer", true)]
            set => _data = value;
        }

        #endregion Data 
    }
}
