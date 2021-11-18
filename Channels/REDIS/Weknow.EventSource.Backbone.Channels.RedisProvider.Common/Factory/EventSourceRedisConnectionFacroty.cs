using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using Weknow.EventSource.Backbone;

using static Weknow.EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;


namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Event Source connection (for IoC)
    /// Because IConnectionMultiplexer may be used by other component, 
    /// It's more clear to wrap the IConnectionMultiplexer for easier resove by IoC.
    /// This factory is also responsible of the connection health.
    /// It will return same connection as long as it healthy.
    /// </summary>
    public sealed class EventSourceRedisConnectionFacroty : IEventSourceRedisConnectionFacroty, IDisposable, IAsyncDisposable
    {
        enum ConnectionState
        {
            On,
            Off
        }


        private Task<IConnectionMultiplexer> _redisTask;
        private readonly ILogger _logger;
        private readonly Action<ConfigurationOptions>? _configuration;
        private readonly RedisCredentialsKeys? _credentialsKeys;

        #region Ctor

        #region Overloads

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        /// <param name="credentialsKeys">Environment keys of the credentials</param>
        public EventSourceRedisConnectionFacroty(
            ILogger<EventSourceRedisConnectionFacroty> logger,
            Action<ConfigurationOptions>? configuration = null,
            RedisCredentialsKeys credentialsKeys = default
            ) : this((ILogger)logger, configuration, credentialsKeys)
        {
        }

        #endregion // Overloads

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        /// <param name="credentialsKeys">Environment keys of the credentials</param>
        public EventSourceRedisConnectionFacroty(
            ILogger logger,
            Action<ConfigurationOptions>? configuration = null,
            RedisCredentialsKeys credentialsKeys = default)
        {
            _logger = logger;
            _configuration = configuration;
            _credentialsKeys = credentialsKeys;
            _redisTask = RedisClientFactory.CreateProviderAsync(logger, configuration, credentialsKeys);
        }


        #endregion // Ctor

        #region GetAsync

        /// <summary>
        /// Get a valid connection 
        /// </summary>
        Task<IConnectionMultiplexer> IEventSourceRedisConnectionFacroty.GetAsync() => _redisTask;

        #endregion // GetAsync

        #region GetAsync

        /// <summary>
        /// Get database 
        /// </summary>
        async Task<IDatabaseAsync> IEventSourceRedisConnectionFacroty.GetDatabaseAsync() 
        {
            IEventSourceRedisConnectionFacroty self = this;
            IConnectionMultiplexer conn = await self.GetAsync();
            IDatabaseAsync db = conn.GetDatabase();
            return db;
        }

        #endregion // GetAsync

        #region Dispose (pattern)

        /// <summary>
        /// Disposed indication
        /// </summary>
        public bool Disposed { get; private set; }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            _logger.LogWarning("REDIS: Disposing connection");
            if (!Disposed)
            {
                var conn = _redisTask.Result;
                conn.Dispose();
                Disposed = true;
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            _logger.LogWarning("REDIS: Disposing connection (async)");
            var redis = await _redisTask;
            redis.Dispose();
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~EventSourceRedisConnectionFacroty()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        #endregion // Dispose (pattern)
    }
}
