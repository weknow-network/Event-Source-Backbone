using StackExchange.Redis;

namespace Weknow.EventSource.Backbone
{
    public interface IEventSourceRedisConnectionFacroty
    {
        Task<IConnectionMultiplexer> CreateAsync();
    }
}