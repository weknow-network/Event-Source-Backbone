using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{

    /// <summary>
    /// In-Memory Channel (excellent for testing)
    /// </summary>
    /// <seealso cref="Weknow.EventSource.Backbone.IProducerChannelProvider" />
    public class ProducerTestChannel :
                            IProducerChannelProvider
    {
        private readonly Channel<Announcement> _channel;
        private int _index;

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
        public async ValueTask<string> SendAsync(Announcement payload)
        {
            await _channel.Writer.WriteAsync(payload);
            return Interlocked.Increment(ref _index).ToString();
        }

        #endregion // SendAsync
    }
}
