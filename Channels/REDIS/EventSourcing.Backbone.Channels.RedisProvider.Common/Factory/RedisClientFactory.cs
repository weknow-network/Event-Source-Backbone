using System.Reflection;
using System.Text;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using static EventSourcing.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// REDIS client factory
    /// </summary>
    public static class RedisClientFactory
    {
        private static int _index = 0;
        private const string CONNECTION_NAME_PATTERN = "ev-src:{0}:{1}:{2}";
        private static readonly string? ASSEMBLY_NAME = Assembly.GetEntryAssembly()?.GetName()?.Name?.ToDash();
        private static readonly Version? ASSEMBLY_VERSION = Assembly.GetEntryAssembly()?.GetName()?.Version;

        #region CreateConfigurationOptions

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
        /// <param name="configurationHook">A configuration hook.</param>
        /// <returns></returns>
        public static ConfigurationOptions CreateConfigurationOptions(
                    string? endpoint = null,
                    string? password = null,
                    Action<ConfigurationOptions>? configurationHook = null)
        {
            RedisCredentialsKeys credential = new RedisCredentialsKeys { Endpoint = endpoint };
            if (!string.IsNullOrEmpty(password))
                credential = credential with { Password = password };
            var redis = credential.CreateConfigurationOptions(configurationHook);
            return redis;
        }

        /// <summary>
        /// Create REDIS configuration options.
        /// </summary>
        /// <param name="credential">The credential's environment keys.</param>
        /// <param name="configurationHook">A configuration hook.</param>
        /// <returns></returns>
        public static ConfigurationOptions CreateConfigurationOptions(
                    this RedisCredentialsKeys credential,
                    Action<ConfigurationOptions>? configurationHook = null)
        {
            string endpointKey = credential.Endpoint;
            string passwordKey = credential.Password;
            string endpoint = Environment.GetEnvironmentVariable(endpointKey) ?? endpointKey;
            string? password = Environment.GetEnvironmentVariable(passwordKey) ?? passwordKey;

            var sb = new StringBuilder();
            var writer = new StringWriter(sb);

            // https://stackexchange.github.io/StackExchange.Redis/Configuration.html
            var configuration = ConfigurationOptions.Parse(endpoint);
            configuration.Password = password;
            if (configurationHook != null)
                configuration.Apply(configurationHook);
            return configuration;
        }

        #endregion // CreateConfigurationOptions

        #region CreateProviderAsync

        /// <summary>
        /// Create REDIS client.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="password">The password.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="configurationHook">A configuration hook.</param>
        /// <returns></returns>
        public static async Task<IConnectionMultiplexer> CreateProviderAsync(
                    string? endpoint = null,
                    string? password = null,
                    ILogger? logger = null,
                    Action<ConfigurationOptions>? configurationHook = null)
        {
            RedisCredentialsKeys credential = new RedisCredentialsKeys { Endpoint = endpoint };
            if (!string.IsNullOrEmpty(password))
                credential = credential with { Password = password };
            var redis = await credential.CreateProviderAsync(logger, configurationHook);
            return redis;
        }

        /// <summary>
        /// Create REDIS client.
        /// </summary>
        /// <param name="credential">The credential's environment keys.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="configurationHook">A configuration hook.</param>
        /// <returns></returns>
        public static async Task<IConnectionMultiplexer> CreateProviderAsync(
                    this RedisCredentialsKeys credential,
                    ILogger? logger = null,
                    Action<ConfigurationOptions>? configurationHook = null)
        {
            string endpointKey = credential.Endpoint;
            string passwordKey = credential.Password;
            string endpoint = Environment.GetEnvironmentVariable(endpointKey) ?? endpointKey;

            try
            {
                string? password = Environment.GetEnvironmentVariable(passwordKey) ?? passwordKey;

                var sb = new StringBuilder();
                var writer = new StringWriter(sb);

                // https://stackexchange.github.io/StackExchange.Redis/Configuration.html
                var configuration = ConfigurationOptions.Parse(endpoint);
                configuration.Password = password;
                if (configurationHook != null)
                    configuration.Apply(configurationHook);
                var redis = await configuration.CreateProviderAsync(logger);
                return redis;
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.LogError(ex.FormatLazy(), "REDIS CONNECTION Setting ERROR: {endpoint}", endpoint);
                else
                    Console.WriteLine($"REDIS CONNECTION Setting ERROR: {ex.FormatLazy()}");
                throw;
            }
            #region Event Handlers

            void OnInternalConnError(object? sender, InternalErrorEventArgs e)
            {
                if (logger != null)
                {
                    logger.LogError(e.Exception, "REDIS Connection internal failure: Failure type = {typeOfConnection}, Origin = {typeOfFailure}",
                                                 e.ConnectionType, e.Origin);
                }
                else
                    Console.WriteLine($"REDIS Connection internal failure: Failure type = {e.ConnectionType}, Origin = {e.Origin}");
            }

            void OnConnErrorMessage(object? sender, RedisErrorEventArgs e)
            {
                if (logger != null)
                {
                    logger.LogWarning("REDIS Connection error: {message}",
                                        e.Message);
                }
                else
                    Console.WriteLine($"REDIS Connection error: {e.Message}");
            }


            void OnConnectionFailed(object? sender, ConnectionFailedEventArgs e)
            {
                if (logger != null)
                {
                    logger.LogError(e.Exception, "REDIS Connection failure: Failure type = {typeOfConnection}, Failure type = {typeOfFailure}", e.ConnectionType, e.FailureType);
                }
                else
                    Console.WriteLine($"REDIS Connection failure: Failure type = {e.ConnectionType}, Failure type = {e.FailureType}");
            }

            #endregion // Event Handlers
        }

        /// <summary>
        /// Create REDIS client.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="credential">The credential's environment keys.</param>
        /// <returns></returns>
        /// <exception cref="StringBuilder">
        /// </exception>
        /// <exception cref="StackExchange.Redis.RedisConnectionException">Fail to establish REDIS connection</exception>
        /// <exception cref="RedisConnectionException">Fail to establish REDIS connection</exception>
        public static async Task<IConnectionMultiplexer> CreateProviderAsync(
                    this ConfigurationOptions configuration,
                    ILogger? logger = null)
        {
            try
            {
                var sb = new StringBuilder();
                var writer = new StringWriter(sb);

                // https://stackexchange.github.io/StackExchange.Redis/Configuration.html
                configuration.ClientName = string.Format(
                                        CONNECTION_NAME_PATTERN,
                                        ASSEMBLY_NAME,
                                        ASSEMBLY_VERSION,
                                        Interlocked.Increment(ref _index));

                configuration = configuration.Apply(cfg =>
                {
                    // keep retry to get connection on failure
                    cfg.AbortOnConnectFail = false;
                    //cfg.ConnectTimeout = 15;
                    //cfg.SyncTimeout = 10;
                    //cfg.AsyncTimeout = 10;
                    //cfg.DefaultDatabase = Debugger.IsAttached ? 1 : null;
                });


                IConnectionMultiplexer redis = await ConnectionMultiplexer.ConnectAsync(configuration, writer);
                string endpoints = string.Join(";", configuration.EndPoints);
                if (logger != null)
                    logger.LogInformation("REDIS Connection [{envKey}]: {info} succeed",
                        endpoints,
                        sb);
                else
                    Console.WriteLine($"REDIS Connection [{endpoints}] succeed: {sb}");
                redis.ConnectionFailed += OnConnectionFailed;
                redis.ErrorMessage += OnConnErrorMessage;
                redis.InternalError += OnInternalConnError;

                return redis;
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.LogError(ex.FormatLazy(), "REDIS CONNECTION ERROR");
                else
                    Console.WriteLine($"REDIS CONNECTION ERROR: {ex.FormatLazy()}");
                throw;
            }

            #region Event Handlers

            void OnInternalConnError(object? sender, InternalErrorEventArgs e)
            {
                if (logger != null)
                {
                    logger.LogError(e.Exception, "REDIS Connection internal failure: Failure type = {typeOfConnection}, Origin = {typeOfFailure}",
                                                 e.ConnectionType, e.Origin);
                }
                else
                    Console.WriteLine($"REDIS Connection internal failure: Failure type = {e.ConnectionType}, Origin = {e.Origin}");
            }

            void OnConnErrorMessage(object? sender, RedisErrorEventArgs e)
            {
                if (logger != null)
                {
                    logger.LogWarning("REDIS Connection error: {message}",
                                        e.Message);
                }
                else
                    Console.WriteLine($"REDIS Connection error: {e.Message}");
            }


            void OnConnectionFailed(object? sender, ConnectionFailedEventArgs e)
            {
                if (logger != null)
                {
                    logger.LogError(e.Exception, "REDIS Connection failure: Failure type = {typeOfConnection}, Failure type = {typeOfFailure}", e.ConnectionType, e.FailureType);
                }
                else
                    Console.WriteLine($"REDIS Connection failure: Failure type = {e.ConnectionType}, Failure type = {e.FailureType}");
            }

            #endregion // Event Handlers

        }

        #endregion // CreateProviderAsync
    }
}
