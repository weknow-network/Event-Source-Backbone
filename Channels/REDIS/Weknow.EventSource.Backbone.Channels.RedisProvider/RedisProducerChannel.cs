using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    public class RedisProducerChannel : IProducerChannelProvider
    {
        public RedisProducerChannel()
        {

        }

        public ValueTask<string> SendAsync(AnnouncementRaw payload)
        {
            throw new System.NotImplementedException();
        }
    }
}
