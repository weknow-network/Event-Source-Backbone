using Microsoft.Extensions.Logging;

using Polly;

using StackExchange.Redis;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
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
        private static readonly TimeSpan RETRY_INTERVAL_DELAY = TimeSpan.FromMilliseconds(200);
        private static readonly TimeSpan MAX_RETRY_INTERVAL_DELAY = TimeSpan.FromMilliseconds(2000);
        private const int RETRY_COUNT = 50;
        private static int _index = 0;
        private const string CONNECTION_NAME_PATTERN = "Event_Source_Producer_{0}";

        /// <summary>
        /// Blocking Create REDIS client.
        /// Exist only for code which don't support async (like ASP.NET setup (AddSingleton))
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="endpointKey">The endpoint key.</param>
        /// <param name="passwordKey">The password key.</param>
        /// <returns></returns>
        /// <exception cref="StackExchange.Redis.RedisConnectionException">Fail to establish REDIS connection</exception>
        /// <exception cref="RedisConnectionException">Fail to establish REDIS connection</exception>
        public static IConnectionMultiplexer CreateProviderBlocking(
                    Action<ConfigurationOptions>? configuration = null,
                    string endpointKey = CONSUMER_END_POINT_KEY,
                    string passwordKey = CONSUMER_PASSWORD_KEY)
        {
            var task = CreateProviderLocalAsync(null, 1, configuration, endpointKey, passwordKey);
            return task.Result;
        }

        /// <summary>
        /// Blocking Create REDIS client.
        /// Exist only for code which don't support async (like ASP.NET setup (AddSingleton))
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="endpointKey">The endpoint key.</param>
        /// <param name="passwordKey">The password key.</param>
        /// <returns></returns>
        /// <exception cref="StackExchange.Redis.RedisConnectionException">Fail to establish REDIS connection</exception>
        /// <exception cref="RedisConnectionException">Fail to establish REDIS connection</exception>
        public static IConnectionMultiplexer CreateProviderBlocking(
                    ILogger logger,
                    Action<ConfigurationOptions>? configuration = null,
                    string endpointKey = CONSUMER_END_POINT_KEY,
                    string passwordKey = CONSUMER_PASSWORD_KEY)
        {
            var task = CreateProviderLocalAsync(logger, 1, configuration, endpointKey, passwordKey);
            return task.Result;
        }

        /// <summary>
        /// Create REDIS client.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="endpointKey">The endpoint key.</param>
        /// <param name="passwordKey">The password key.</param>
        /// <returns></returns>
        /// <exception cref="StackExchange.Redis.RedisConnectionException">Fail to establish REDIS connection</exception>
        /// <exception cref="RedisConnectionException">Fail to establish REDIS connection</exception>
        public static Task<IConnectionMultiplexer> CreateProviderAsync(
                    Action<ConfigurationOptions>? configuration = null,
                    string endpointKey = CONSUMER_END_POINT_KEY,
                    string passwordKey = CONSUMER_PASSWORD_KEY)
        {
            return CreateProviderLocalAsync(null, RETRY_COUNT, configuration, endpointKey, passwordKey);
        }

        /// <summary>
        /// Create REDIS client.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="endpointKey">The endpoint key.</param>
        /// <param name="passwordKey">The password key.</param>
        /// <returns></returns>
        /// <exception cref="StackExchange.Redis.RedisConnectionException">Fail to establish REDIS connection</exception>
        /// <exception cref="RedisConnectionException">Fail to establish REDIS connection</exception>
        public static Task<IConnectionMultiplexer> CreateProviderAsync(
                    ILogger logger,
                    Action<ConfigurationOptions>? configuration = null,
                    string endpointKey = CONSUMER_END_POINT_KEY,
                    string passwordKey = CONSUMER_PASSWORD_KEY)
        {
            return CreateProviderLocalAsync(logger, RETRY_COUNT, configuration, endpointKey, passwordKey);
        }

        /// <summary>
        /// Create REDIS client.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="numberOfRetries">The number of retries.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="endpointKey">The endpoint key.</param>
        /// <param name="passwordKey">The password key.</param>
        /// <returns></returns>
        /// <exception cref="StringBuilder">
        /// </exception>
        /// <exception cref="StackExchange.Redis.RedisConnectionException">Fail to establish REDIS connection</exception>
        /// <exception cref="RedisConnectionException">Fail to establish REDIS connection</exception>
        private static async Task<IConnectionMultiplexer> CreateProviderLocalAsync(
                    ILogger? logger,
                    int numberOfRetries,
                    Action<ConfigurationOptions>? configuration = null,
                    string endpointKey = CONSUMER_END_POINT_KEY,
                    string passwordKey = CONSUMER_PASSWORD_KEY)
        {
            string? endpoint = Environment.GetEnvironmentVariable(endpointKey);

            try
            {
                if (endpoint == null)
                    throw new KeyNotFoundException($"REDIS KEY [{endpointKey}] is missing from env variables");

                string? password = Environment.GetEnvironmentVariable(passwordKey);

                var sb = new StringBuilder();
                var writer = new StringWriter(sb);

                var redisConfiguration = ConfigurationOptions.Parse(endpoint);
                redisConfiguration.ClientName = string.Format(
                                        CONNECTION_NAME_PATTERN,
                                        Interlocked.Increment(ref _index));

                configuration?.Invoke(redisConfiguration);
                redisConfiguration.Password = password;

                IConnectionMultiplexer redis;
                TimeSpan delay = RETRY_INTERVAL_DELAY;
                for (int i = 1; i <= numberOfRetries; i++)
                {
                    try
                    {
                        redis = await ConnectionMultiplexer.ConnectAsync(redisConfiguration, writer);
                        if (logger != null)
                            logger.LogInformation(sb.ToString());
                        else
                            Console.WriteLine(sb);
                        return redis;
                    }
                    catch (Exception ex)
                    {
                        writer.Flush();
                        if (i >= RETRY_COUNT)
                        {
                            string msg = $"Fail to create REDIS client [final retry ({i})]. {Environment.NewLine}{sb}";
                            if (logger != null)
                                logger.LogError(ex.FormatLazy(), msg);
                            else
                                Console.WriteLine(msg);
                            throw;
                        }
                        if (i % 10 == 0)
                        {
                            string msg = $"Fail to create REDIS client [retry = {i}]. {Environment.NewLine}{sb}";
                            if (logger != null)
                                logger.LogWarning(ex.FormatLazy(), msg);
                            else
                                Console.WriteLine(msg);
                        }
                        if (delay < MAX_RETRY_INTERVAL_DELAY)
                        {
                            delay = delay.Add(delay);
                            await Task.Delay(delay);
                        }
                        else
                            await Task.Delay(MAX_RETRY_INTERVAL_DELAY);
                    }
                }
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.LogError(ex.FormatLazy());
                else
                    Console.WriteLine($"REDIS CONNECTION ERROR: {ex.FormatLazy()}");
            }

            string err = $"Fail to establish REDIS connection [env: {endpointKey}, endpoint:{endpoint}]";
            if (logger != null)
                logger.LogError(err);
            else
                Console.WriteLine(err);
            throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, err);
        }


        /// <summary>
        /// Create REDIS database client.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="endpointKey">The endpoint key.</param>
        /// <param name="passwordKey">The password key.</param>
        /// <returns></returns>
        /// <exception cref="StackExchange.Redis.RedisConnectionException">Fail to establish REDIS connection</exception>
        /// <exception cref="RedisConnectionException">Fail to establish REDIS connection</exception>
        public static async Task<IDatabaseAsync> CreateAsync(
                    Action<ConfigurationOptions>? configuration = null,
                    string endpointKey = CONSUMER_END_POINT_KEY,
                    string passwordKey = CONSUMER_PASSWORD_KEY)
        {
            var provider = await CreateProviderLocalAsync(null, RETRY_COUNT, configuration, endpointKey, passwordKey);
            return provider.GetDatabase();
        }

        /// <summary>
        /// Create REDIS database client.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="endpointKey">The endpoint key.</param>
        /// <param name="passwordKey">The password key.</param>
        /// <returns></returns>
        /// <exception cref="StackExchange.Redis.RedisConnectionException">Fail to establish REDIS connection</exception>
        /// <exception cref="RedisConnectionException">Fail to establish REDIS connection</exception>
        public static async Task<IDatabaseAsync> CreateAsync(
                    ILogger logger,
                    Action<ConfigurationOptions>? configuration = null,
                    string endpointKey = CONSUMER_END_POINT_KEY,
                    string passwordKey = CONSUMER_PASSWORD_KEY)
        {
            var provider = await CreateProviderLocalAsync(logger, RETRY_COUNT, configuration, endpointKey, passwordKey);
            return provider.GetDatabase();
        }
    }
}
