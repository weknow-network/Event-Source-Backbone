using System.Threading.Tasks;

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
    public class EventSourceRedisConnectionFacroty : IEventSourceRedisConnectionFacroty
    {
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
        /// <param name="credentialsKeys"></param>
        public EventSourceRedisConnectionFacroty(
            ILogger<EventSourceRedisConnectionFacroty> logger,
            Action<ConfigurationOptions>? configuration = null,
            RedisCredentialsKeys? credentialsKeys = null
            ): this((ILogger)logger, configuration, credentialsKeys)
        {
        }

        #endregion // Overloads

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        /// <param name="credentialsKeys"></param>
        public EventSourceRedisConnectionFacroty(
            ILogger logger,
            Action<ConfigurationOptions>? configuration = null,
            RedisCredentialsKeys? credentialsKeys = null
            )
        {
            _logger = logger;
            _configuration = configuration;
            _credentialsKeys = credentialsKeys;
            _redisTask = RedisClientFactory.CreateProviderAsync(
                                                _logger,
                                                _configuration,
                                                _credentialsKeys?.EndpointKey ?? END_POINT_KEY,
                                                _credentialsKeys?.PasswordKey ?? PASSWORD_KEY);
            MonitorHealth();
        }


        #endregion // Ctor

        /// <summary>
        /// Async connection
        /// </summary>
        public async Task<IConnectionMultiplexer> CreateAsync()
        {
            IConnectionMultiplexer redis = await _redisTask;
            while (!redis.IsConnected)
            {
                try
                {
                    #region Ping Validation

                    TimeSpan ping = await redis.GetDatabase().PingAsync();
                    if (redis.IsConnected)
                        break;

                    #endregion // Ping Validation

                    _logger.LogWarning("REDIS Client [{name}] is not connected", redis.ClientName);
                    _redisTask = RedisClientFactory.CreateProviderAsync(_logger, _configuration);
                    redis = await _redisTask;

                }
                #region Exception Handling

                catch (RedisConnectionException ex)
                {
                    _logger.LogWarning(ex, "REDIS: Fault Client [{name}] waiting for connection", redis.ClientName);
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "REDIS: Bad Client [{name}] waiting for connection", redis.ClientName);
                }

                #endregion // Exception Handling
            }
            return redis;
        }

        #region MonitorHealth

        /// <summary>
        /// Monitor Health
        /// </summary>
        private void MonitorHealth()
        {
            _redisTask.ContinueWith(t =>
            {
                IConnectionMultiplexer conn = t.Result;
                conn.ConnectionFailed += OnConnectionFailed;
                conn.ErrorMessage += OnConnErrorMessage;
                conn.InternalError += OnInternalConnError;
            });
        }

        #endregion // MonitorHealth

        #region RemoveConnection

        /// <summary>
        /// Clean connectin resouces
        /// </summary>
        private void RemoveConnection()
        {
            _redisTask.ContinueWith(t =>
            {
                IConnectionMultiplexer conn = t.Result;
                conn.ConnectionFailed -= OnConnectionFailed;
                conn.ErrorMessage -= OnConnErrorMessage;
                conn.InternalError -= OnInternalConnError;
            });
            _redisTask.Dispose();
        }

        #endregion // RemoveConnection

        #region OnInternalConnError

        /// <summary>
        /// When having internal error
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnInternalConnError(object? sender, InternalErrorEventArgs e)
        {
            RemoveConnection();
            _logger.LogError(e.Exception, "REDIS Connection internal failure: Failure type = {typeOfConnection}, Origin = {typeOfFailure}", 
                                         e.ConnectionType, e.Origin);
            _redisTask = RedisClientFactory.CreateProviderAsync(
                                                _logger,
                                                _configuration,
                                                _credentialsKeys?.EndpointKey ?? END_POINT_KEY,
                                                _credentialsKeys?.PasswordKey ?? PASSWORD_KEY);
            MonitorHealth();
        }

        #endregion // OnInternalConnError

        #region OnConnErrorMessage

        /// <summary>
        /// When having error message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnErrorMessage(object? sender, RedisErrorEventArgs e)
        {
            //UnonitorHealth();
            _logger.LogWarning("REDIS Connection error: {message}", 
                                e.Message);
            //_redisTask = RedisClientFactory.CreateProviderAsync(
            //                                    _logger,
            //                                    _configuration,
            //                                    _credentialsKeys?.EndpointKey ?? END_POINT_KEY,
            //                                    _credentialsKeys?.PasswordKey ?? PASSWORD_KEY);
            //MonitorHealth();
        }

        #endregion // OnConnErrorMessage

        #region OnConnectionFailed

        /// <summary>
        /// When Connection Failed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionFailed(object? sender, ConnectionFailedEventArgs e)
        {
            RemoveConnection();
            _logger.LogError(e.Exception, "REDIS Connection failure: Failure type = {typeOfConnection}, Failure type = {typeOfFailure}", e.ConnectionType, e.FailureType);
            _redisTask = RedisClientFactory.CreateProviderAsync(
                                                _logger,
                                                _configuration,
                                                _credentialsKeys?.EndpointKey ?? END_POINT_KEY,
                                                _credentialsKeys?.PasswordKey ?? PASSWORD_KEY);
            MonitorHealth();
        }

        #endregion // OnConnectionFailed
    }
}
