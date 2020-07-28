using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    public class RedisProducerChannel : IProducerChannelProvider
    {
        public RedisProducerChannel()
        {

        }

        public ValueTask SendAsync(Announcement payload)
        {
            throw new System.NotImplementedException();
        }
    }
}
