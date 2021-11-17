using StackExchange.Redis;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Connection factory
    /// </summary>
    public interface IEventSourceRedisConnectionFacroty
    {
        /// <summary>
        /// Get a valid connection 
        /// </summary>
        Task<IConnectionMultiplexer> CreateAsync();
        /// <summary>
        /// Reset
        /// </summary>
        /// <returns></returns>
        Task ResetAsync();
    }
}