using System.Net;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;


namespace EventSourcing.Backbone
{
    /// <summary>
    /// Event Source connection (for IoC)
    /// Because IConnectionMultiplexer may be used by other component, 
    /// It's more clear to wrap the IConnectionMultiplexer for easier resove by IoC.
    /// This factory is also responsible of the connection health.
    /// It will return same connection as long as it healthy.
    /// </summary>
    public abstract class RedisConnectionFacrotyBase : IEventSourceRedisConnectionFacroty, IDisposable, IAsyncDisposable
    {
        private const int CLOSE_DELEY_MILLISECONDS = 5000;
        private Task<IConnectionMultiplexer> _redisTask;
        private readonly ILogger _logger;
        private readonly ConfigurationOptions _configuration;
        private readonly AsyncLock _lock = new AsyncLock(TimeSpan.FromSeconds(CLOSE_DELEY_MILLISECONDS));
        private DateTime _lastResetConnection = DateTime.Now;
        private int _reconnectTry = 0;

        #region Ctor

        #region Overloads

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <summary>
        /// Create REDIS configuration options.
        /// </summary>
        /// <param name="endpoint">
        /// Environment key of the end-point, if missing it use a default ('REDIS_EVENT_SOURCE_ENDPOINT').
        /// If the environment variable doesn't exists, It assumed that the value represent an actual end-point and use it.
        /// </param>
        /// <param name="password">
        /// Environment key of the password, if missing it use a default ('REDIS_EVENT_SOURCE_PASS').
        /// If the environment variable doesn't exists, It assumed that the value represent an actual password and use it.
        /// </param>
        /// <param name="configurationHook">The configuration hook.</param>
        protected RedisConnectionFacrotyBase(
                    ILogger logger,
                    string? endpoint = null,
                    string? password = null,
                    Action<ConfigurationOptions>? configurationHook = null)
        {
            _logger = logger;
            _configuration = RedisClientFactory.CreateConfigurationOptions(endpoint, password, configurationHook);
            _redisTask = RedisClientFactory.CreateProviderAsync(_configuration, logger);
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="credential">The credential.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="configurationHook">The configuration hook.</param>
        protected RedisConnectionFacrotyBase(
                    RedisCredentialsKeys credential,
                    ILogger logger,
                    Action<ConfigurationOptions>? configurationHook = null)
        {
            _logger = logger;
            _configuration = credential.CreateConfigurationOptions(configurationHook);
            _redisTask = RedisClientFactory.CreateProviderAsync(_configuration, logger);
        }


        #endregion // Overloads

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        protected RedisConnectionFacrotyBase(
            ILogger logger,
            ConfigurationOptions? configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _redisTask = RedisClientFactory.CreateProviderAsync(configuration, logger);
        }


        #endregion // Ctor

        #region Kind

        /// <summary>
        /// Gets the kind.
        /// </summary>
        protected abstract string Kind { get; }

        #endregion // Kind

        #region GetAsync

        /// <summary>
        /// Get a valid connection 
        /// </summary>
        async Task<IConnectionMultiplexer> IEventSourceRedisConnectionFacroty.GetAsync()
        {
            var conn = await _redisTask;
            if (conn.IsConnected)
                return conn;
            string status = conn.GetStatus();
            _logger.LogWarning("REDIS Connection [{kind}] [{ClientName}]: status = [{status}]",
                                Kind,
                                conn.ClientName, status);
            var disp = await _lock.AcquireAsync();
            using (disp)
            {
                conn = await _redisTask;
                if (conn.IsConnected)
                    return conn;
                int tryNumber = Interlocked.Increment(ref _reconnectTry);
                _logger.LogWarning("[{kind}] Reconnecting to REDIS: try=[{tryNumber}], client name=[{clientName}]",
                                           Kind, tryNumber, conn.ClientName);
                var duration = DateTime.Now - _lastResetConnection;
                if (duration > TimeSpan.FromSeconds(5))
                {
                    _lastResetConnection = DateTime.Now;
                    var cn = conn;
                    Task _ = Task.Delay(CLOSE_DELEY_MILLISECONDS).ContinueWith(_ => cn.CloseAsync());
                    _redisTask = _configuration.CreateProviderAsync(_logger);
                    var newConn = await _redisTask;
                    return newConn;
                }
                return conn;
            }
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
            _logger.LogWarning("REDIS [{kind}]: Disposing connection", Kind);
            if (!Disposed)
            {
                var conn = _redisTask.Result;
                conn.Dispose();
                Disposed = true;
                OnDispose(disposing);
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
        /// Called when [dispose].
        /// </summary>
        /// <param name="disposing">if set to <c>true</c> [disposing].</param>
        /// <returns></returns>
        public virtual void OnDispose(bool disposing) { }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            _logger.LogWarning("REDIS [{kind}]: Disposing connection (async)", Kind);
            var redis = await _redisTask;
            redis.Dispose();
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~RedisConnectionFacrotyBase()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        #endregion // Dispose (pattern)
    }
}
