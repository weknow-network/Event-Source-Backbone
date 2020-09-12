using StackExchange.Redis;

using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    /// <summary>
    /// REDIS Client Factory
    /// </summary>
    public interface IRedisClientFactory
    {
        /// <summary>
        /// Creates Multiplexer.
        /// </summary>
        /// <returns></returns>
        Task<ConnectionMultiplexer> CreateAsync();
        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <returns></returns>
        Task<IDatabaseAsync> GetDbAsync();
        ///// <summary>
        ///// Gets the subscriber.
        ///// </summary>
        ///// <returns></returns>
        //Task<ISubscriber> GetSubscriberAsync();
    }
}