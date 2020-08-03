using System;
using System.Threading;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    public class RedisConsumerChannel : IConsumerChannelProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RedisConsumerChannel()
        {

        }

        public ValueTask ReceiveAsync(
                        Func<Announcement, ValueTask> func,
                        IEventSourceConsumerOptions options,
                        CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion // Ctor
    }
}
