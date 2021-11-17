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
    public class EventSourceRedisConnectionFacroty : IEventSourceRedisConnectionFacroty
    {
        enum ConnectionState
        {
            On,
            Off
        }

        record ConnSwitch(Task<IConnectionMultiplexer> ConnTask, ConnectionState State);

        private Task<IConnectionMultiplexer> _redisTask;
        private readonly ILogger _logger;
        private readonly Action<ConfigurationOptions>? _configuration;
        private readonly RedisCredentialsKeys? _credentialsKeys;
        private readonly ActionBlock<ConnSwitch> _connListener;

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
            ) : this((ILogger)logger, configuration, credentialsKeys)
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
            var switcher = ConnectionSwitcherAsync;
            _connListener = new(switcher);
            _redisTask = CreateNewAsync();
        }


        #endregion // Ctor

        #region CreateAsync

        /// <summary>
        /// Get a valid connection 
        /// </summary>
        public async Task<IConnectionMultiplexer> CreateAsync()
        {
            IConnectionMultiplexer redis = await _redisTask;
            return redis;
        }

        #endregion // CreateAsync

        #region ResetAsync

        /// <summary>
        /// Reset
        /// </summary>
        /// <returns></returns>
        public async Task ResetAsync()
        {
            var swtc = new ConnSwitch(_redisTask, ConnectionState.Off);
            _connListener.Post(swtc);

            var connTask = CreateNewAsync();
            _redisTask = connTask;
            await connTask;
        }

        #endregion // ResetAsync

        #region CreateNewAsync

        /// <summary>
        /// Create connection
        /// </summary>
        private async Task<IConnectionMultiplexer> CreateNewAsync()
        {
            IConnectionMultiplexer redis;
            int delay = 10;
            for (int i = 1; i <= 10; i++)
            {
                delay *= 2;
                try
                {
                    redis = await RedisClientFactory.CreateProviderAsync(_logger, _configuration);
                    TimeSpan ping = default;
                    try
                    {
                        ping = await redis.GetDatabase().PingAsync();
                    }
                    #region Exception Handling

                    catch 
                    {
                        redis.Dispose();
                        throw;
                    }

                    #endregion // Exception Handling
                    _logger.LogInformation("CREATE REDIS Client [{name}], Ping = {ping}", redis.ClientName, ping);
                    var swtc = new ConnSwitch(Task.FromResult(redis), ConnectionState.On);
                    _connListener.Post(swtc);

                    return redis;
                }
                #region Exception Handling

                catch (RedisConnectionException ex)
                {
                    _logger.LogWarning(ex, "REDIS: Fault Client [try: {try}] waiting for connection", i);
                    await Task.Delay(TimeSpan.FromMilliseconds(delay));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "REDIS: Bad Client [try: {try}]  waiting for connection", i);
                    await Task.Delay(TimeSpan.FromMilliseconds(delay));
                }

                #endregion // Exception Handling
            }
            throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "All connection attempt failed");
        }

        #endregion // CreateNewAsync

        #region ConnectionSwitcherAsync

        /// <summary>
        /// Control connection listening
        /// </summary>
        /// <param name="swch"></param>
        /// <returns></returns>
        private async Task ConnectionSwitcherAsync(ConnSwitch swch)
        {
            IConnectionMultiplexer conn = await swch.ConnTask;
            switch (swch.State)
            {
                case ConnectionState.On:
                    {
                        conn.ConnectionFailed += OnConnectionFailed;
                        conn.ErrorMessage += OnConnErrorMessage;
                        conn.InternalError += OnInternalConnError;
                    }
                    break;
                case ConnectionState.Off:
                    {
                        conn.ConnectionFailed -= OnConnectionFailed;
                        conn.ErrorMessage -= OnConnErrorMessage;
                        conn.InternalError -= OnInternalConnError;
                        await Task.Delay(5);
                        try
                        {
                            conn.Dispose();
                        }
                        catch (Exception)
                        {
                            // ignore
                        }
                    }
                    break;
            }
        }

        #endregion // ConnectionSwitcherAsync

        #region OnInternalConnError

        /// <summary>
        /// When having internal error
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnInternalConnError(object? sender, InternalErrorEventArgs e)
        {
            _logger.LogError(e.Exception, "REDIS Connection internal failure: Failure type = {typeOfConnection}, Origin = {typeOfFailure}",
                                         e.ConnectionType, e.Origin);
            Task _ = ResetAsync();
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
            _logger.LogWarning("REDIS Connection error: {message}",
                                e.Message);
            // Task _ = ResetAsync();
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
            _logger.LogError(e.Exception, "REDIS Connection failure: Failure type = {typeOfConnection}, Failure type = {typeOfFailure}", e.ConnectionType, e.FailureType);
            Task _ = ResetAsync();
        }

        #endregion // OnConnectionFailed
    }
}
