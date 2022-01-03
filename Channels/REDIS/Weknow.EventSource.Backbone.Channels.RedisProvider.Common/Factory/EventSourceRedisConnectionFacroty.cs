using System.Diagnostics;
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
        private const int CLOSE_DELEY_MILLISECONDS = 5000;
        private Task<IConnectionMultiplexer> _redisTask;
        private readonly ILogger _logger;
        private readonly Action<ConfigurationOptions>? _configuration;
        private readonly RedisCredentialsKeys _credentialsKeys;
        private readonly AsyncLock _lock = new AsyncLock(TimeSpan.FromSeconds(CLOSE_DELEY_MILLISECONDS));
        private DateTime _lastResetConnection = DateTime.Now;
        private int _reconnectTry = 0;

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
        async Task<IConnectionMultiplexer> IEventSourceRedisConnectionFacroty.GetAsync()
        {
            var conn = await _redisTask;
            if(conn.IsConnected)
                return conn;
            string status = conn.GetStatus();
            _logger.LogWarning("REDIS Connection [{ClientName}] status = [{status}]",
                                conn.ClientName, status);
            var disp = await _lock.AcquireAsync();
            using (disp)
            {
                conn = await _redisTask;
                if(conn.IsConnected)
                    return conn;
                int tryNumber = Interlocked.Increment(ref _reconnectTry);
                _logger.LogWarning("Reconnecting to REDIS: try=[{tryNumber}], client name=[{clientName}]",
                                            tryNumber, conn.ClientName);
                var duration = DateTime.Now - _lastResetConnection;
                if ( duration > TimeSpan.FromSeconds(5))
                {
                    _lastResetConnection = DateTime.Now;
                    var cn = conn;
                    Task _ = Task.Delay(CLOSE_DELEY_MILLISECONDS).ContinueWith(_ => cn.CloseAsync());
                    _redisTask = RedisClientFactory.CreateProviderAsync(_logger, _configuration, _credentialsKeys);
                    var newConn = await _redisTask;
                    return newConn;
                }
                return conn;
            }
        } 

        /// <summary>
        /// Get a valid connection 
        /// </summary>
        async Task<IConnectionMultiplexer> IEventSourceRedisConnectionFacroty.ReportErrorAsync()
        {
            var conn = await _redisTask;
            conn.Dispose();
            _redisTask = RedisClientFactory.CreateProviderAsync(_logger, _configuration, _credentialsKeys);
            conn = await _redisTask;
            return conn;
        }

        #endregion // GetAsync

        #region GetDatabaseAsync

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

        #endregion // GetDatabaseAsync

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
