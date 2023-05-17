using StackExchange.Redis;

namespace EventSource.Backbone
{
    /// <summary>
    /// Connection factory
    /// </summary>
    public interface IRedisConnectionFacrotyBase
    {
        /// <summary>
        /// Get a valid connection 
        /// </summary>
        Task<IConnectionMultiplexer> GetAsync();
        /// <summary>
        /// Get database 
        /// </summary>
        Task<IDatabaseAsync> GetDatabaseAsync();
    }
}