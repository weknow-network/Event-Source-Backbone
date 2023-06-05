using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using static EventSourcing.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;

#pragma warning disable S3881 // "IDisposable" should be implemented correctly
#pragma warning disable S2953 // Methods named "Dispose" should implement "IDisposable.Dispose"


namespace EventSourcing.Backbone
{
    /// <summary>
    /// Event Source connection (for IoC)
    /// Because IConnectionMultiplexer may be used by other component, 
    /// It's more clear to wrap the IConnectionMultiplexer for easier resove by IoC.
    /// This factory is also responsible of the connection health.
    /// It will return same connection as long as it healthy.
    /// </summary>
    public class EventSourceRedisConnectionFactory : IEventSourceRedisConnectionFactory, IDisposable, IAsyncDisposable
    {
        private const int CLOSE_DELEY_MILLISECONDS = 5000;
        private Task<IConnectionMultiplexer> _redisTask;
        private readonly ILogger _logger;
        private readonly ConfigurationOptions _configuration;
        private readonly AsyncLock _lock = new AsyncLock(TimeSpan.FromSeconds(CLOSE_DELEY_MILLISECONDS));
        private DateTime _lastResetConnection = DateTime.Now;
        private int _reconnectTry = 0;

        #region Ctor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        public EventSourceRedisConnectionFactory(
                    ILogger<IEventSourceRedisConnectionFactory> logger,
                    ConfigurationOptions? configuration = null)
            : this((ILogger)logger, configuration)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        private EventSourceRedisConnectionFactory(
                    ILogger logger,
                    ConfigurationOptions? configuration = null)
        {
            _logger = logger;
            if (configuration == null)
            {
                var cred = new RedisCredentialsEnvKeys();
                _configuration = cred.CreateConfigurationOptions();
            }
            else
            {
                _configuration = configuration;
            }
            _redisTask = RedisClientFactory.CreateProviderAsync(_configuration, logger);
        }

        #endregion // Ctor

        #region Create

        /// <summary>
        /// Create instance
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        public static IEventSourceRedisConnectionFactory Create(
                    ILogger logger,
                    ConfigurationOptions? configuration = null)
        {
            return new EventSourceRedisConnectionFactory(logger, configuration);
        }

        /// <summary>
        /// Create instance
        /// </summary>
        /// <param name="credential">The credential.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="configurationHook">The configuration hook.</param>
        /// <returns></returns>
        public static IEventSourceRedisConnectionFactory Create(
                    IRedisCredentials credential,
                    ILogger logger,
                    Action<ConfigurationOptions>? configurationHook = null)
        {
            var configuration = credential.CreateConfigurationOptions();
            return new EventSourceRedisConnectionFactory(logger, configuration);
        }

        /// <summary>
        /// Create instance
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="endpoint">The raw endpoint (not an environment variable).</param>
        /// <param name="password">The password (not an environment variable).</param>
        /// <param name="configurationHook">The configuration hook.</param>
        /// <returns></returns>
        public static IEventSourceRedisConnectionFactory Create(
                    ILogger logger,
                    string endpoint,
                    string? password = null,
                    Action<ConfigurationOptions>? configurationHook = null)
        {
            var credential = new RedisCredentialsRaw(endpoint, password);
            return Create(credential, logger, configurationHook);
        }

        /// <summary>
        /// Create instance from environment variable
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="endpointEnvKey">The endpoint.</param>
        /// <param name="passwordEnvKey">The password.</param>
        /// <param name="configurationHook">The configuration hook.</param>
        /// <returns></returns>
        public static IEventSourceRedisConnectionFactory CreateFromEnv(
                    ILogger logger,
                    string endpointEnvKey,
                    string passwordEnvKey = PASSWORD_KEY,
                    Action<ConfigurationOptions>? configurationHook = null)
        {
            var credential = new RedisCredentialsEnvKeys(endpointEnvKey, passwordEnvKey);
            return Create(credential, logger, configurationHook);
        }

        #endregion // Create

        #region Kind

        /// <summary>
        /// Gets the kind.
        /// </summary>
        protected virtual string Kind { get; } = "Event-Sourcing";

        #endregion // Kind

        #region GetAsync

        /// <summary>
        /// Get a valid connection 
        /// </summary>
        async Task<IConnectionMultiplexer> IEventSourceRedisConnectionFactory.GetAsync()
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
#pragma warning disable S1481 
                    Task _ = Task.Delay(CLOSE_DELEY_MILLISECONDS).ContinueWith(_ => cn.CloseAsync());
#pragma warning restore S1481
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
        async Task<IDatabaseAsync> IEventSourceRedisConnectionFactory.GetDatabaseAsync()
        {
            IEventSourceRedisConnectionFactory self = this;
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
        void IDisposable.Dispose()
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
        protected virtual void OnDispose(bool disposing) { }

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
        ~EventSourceRedisConnectionFactory()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        #endregion // Dispose (pattern)
    }
}
