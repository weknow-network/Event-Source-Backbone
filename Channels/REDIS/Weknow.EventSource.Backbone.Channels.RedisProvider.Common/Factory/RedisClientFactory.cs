using Microsoft.Extensions.Logging;

using Polly;

using StackExchange.Redis;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Private;

using static Weknow.EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// REDIS client factory
    /// </summary>
    public static class RedisClientFactory
    {
#pragma warning disable ConstFieldDocumentationHeader // The field must have a documentation header.
        private static int _index = 0;
        private const string CONNECTION_NAME_PATTERN = "ev-src:{0}:{1}:{2}";
        private static readonly string? ASSEMBLY_NAME = Assembly.GetEntryAssembly()?.GetName()?.Name?.ToDash();
        private static readonly Version? ASSEMBLY_VERSION = Assembly.GetEntryAssembly()?.GetName()?.Version;
#pragma warning restore ConstFieldDocumentationHeader // The field must have a documentation header.

        /// <summary>
        /// Blocking Create REDIS client.
        /// Exist only for code which don't support async (like ASP.NET setup (AddSingleton))
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="credential">The credential.</param>
        /// <returns></returns>
        /// <exception cref="StackExchange.Redis.RedisConnectionException">Fail to establish REDIS connection</exception>
        /// <exception cref="RedisConnectionException">Fail to establish REDIS connection</exception>
        public static IConnectionMultiplexer CreateProviderBlocking(
                    Action<ConfigurationOptions>? configuration = null,
                    RedisCredentialsKeys credential = default)
        {
            var task = CreateProviderAsync(null, configuration, credential);
            return task.Result;
        }

        /// <summary>
        /// Blocking Create REDIS client.
        /// Exist only for code which don't support async (like ASP.NET setup (AddSingleton))
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="credential">The credential.</param>
        /// <returns></returns>
        /// <exception cref="StackExchange.Redis.RedisConnectionException">Fail to establish REDIS connection</exception>
        /// <exception cref="RedisConnectionException">Fail to establish REDIS connection</exception>
        public static IConnectionMultiplexer CreateProviderBlocking(
                    ILogger logger,
                    Action<ConfigurationOptions>? configuration = null,
                    RedisCredentialsKeys credential = default)
        {
            var task = CreateProviderAsync(logger, configuration, credential);
            return task.Result;
        }

        /// <summary>
        /// Create REDIS client.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="credential">The credential.</param>
        /// <returns></returns>
        /// <exception cref="StackExchange.Redis.RedisConnectionException">Fail to establish REDIS connection</exception>
        /// <exception cref="RedisConnectionException">Fail to establish REDIS connection</exception>
        public static Task<IConnectionMultiplexer> CreateProviderAsync(
                    Action<ConfigurationOptions>? configuration = null,
                    RedisCredentialsKeys credential = default)
        {
            return CreateProviderAsync(null, configuration, credential);
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
                    ILogger? logger,
                    Action<ConfigurationOptions>? configuration = null,
                    RedisCredentialsKeys credential = default)
        {
            string endpointKey = credential.EndpointKey ?? END_POINT_KEY;
            string passwordKey = credential.PasswordKey ?? PASSWORD_KEY;
            string? endpoint = Environment.GetEnvironmentVariable(endpointKey);

            try
            {
                if (endpoint == null)
                {
                    #region Throw + Log

                    if (logger != null)
                        logger.LogError("REDIS CONNECTION: ENDPOINT [ENV variable: {endpointKey}] is missing", endpointKey);
                    else
                        Console.WriteLine($"REDIS CONNECTION: ENDPOINT [ENV variable: {endpointKey}] is missing");
                    throw new KeyNotFoundException($"REDIS KEY [ENV variable: {endpointKey}] is missing");

                    #endregion // Throw + Log
                }

                string? password = Environment.GetEnvironmentVariable(passwordKey);

                var sb = new StringBuilder();
                var writer = new StringWriter(sb);

                // https://stackexchange.github.io/StackExchange.Redis/Configuration.html
                var redisConfiguration = ConfigurationOptions.Parse(endpoint);
                redisConfiguration.ClientName = string.Format(
                                        CONNECTION_NAME_PATTERN,
                                        ASSEMBLY_NAME,
                                        ASSEMBLY_VERSION,
                                        Interlocked.Increment(ref _index));

                configuration?.Invoke(redisConfiguration);
                redisConfiguration.Password = password;
                // keep retry to get connection on failure
                redisConfiguration.AbortOnConnectFail = false;
                //redisConfiguration.ConnectTimeout = 15;
                //redisConfiguration.SyncTimeout = 10;
                //redisConfiguration.AsyncTimeout = 10;
                //redisConfiguration.DefaultDatabase = Debugger.IsAttached ? 1 : null;


                IConnectionMultiplexer redis = await ConnectionMultiplexer.ConnectAsync(redisConfiguration, writer);
                if (logger != null)
                    logger.LogInformation("REDIS Connection [{envKey}]: {info} succeed", endpointKey, sb);
                else
                    Console.WriteLine($"REDIS Connection [{endpointKey}] succeed: {sb}");
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
    }
}
