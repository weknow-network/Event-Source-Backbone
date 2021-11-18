﻿using StackExchange.Redis;

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
        Task<IConnectionMultiplexer> GetAsync();
        /// <summary>
        /// Get database 
        /// </summary>
        Task<IDatabaseAsync> GetDatabaseAsync();
    }
}