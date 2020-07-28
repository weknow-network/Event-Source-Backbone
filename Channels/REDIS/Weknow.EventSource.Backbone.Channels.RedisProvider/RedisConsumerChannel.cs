using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

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

        public void Init(IEventSourceConsumerOptions options)
        {
            throw new NotImplementedException();
        }

        public ValueTask ReceiveAsync(Func<Announcement, ValueTask> func)
        {
            throw new NotImplementedException();
        }

        #endregion // Ctor
    }
}
