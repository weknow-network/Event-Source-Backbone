using System;
using System.Collections;
using System.Collections.Immutable;
using System.Threading.Channels;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{

    /// <summary>
    /// In-Memory Channel (excelent for testing)
    /// </summary>
    /// <seealso cref="Weknow.EventSource.Backbone.IProducerChannelProvider" />
    public class ProducerTestChannel :
                            IProducerChannelProvider
    {
        private readonly Channel<Announcement> _channel;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="channel">The channel.</param>
        public ProducerTestChannel(Channel<Announcement> channel)
        {
            _channel = channel;
        }

        #endregion // Ctor

        #region SendAsync

        /// <summary>
        /// Sends raw announcement.
        /// </summary>
        /// <param name="payload">The raw announcement data.</param>
        /// <returns>
        /// The announcement id
        /// </returns>
        public async ValueTask SendAsync(Announcement payload)
        {
            await _channel.Writer.WriteAsync(payload);
        }

        #endregion // SendAsync
    }
}
