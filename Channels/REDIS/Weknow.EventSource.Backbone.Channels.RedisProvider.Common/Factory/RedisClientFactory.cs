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

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    /// <summary>
    /// REDIS client factory
    /// </summary>
    public static class RedisClientFactory
    {
        private static readonly TimeSpan RETRY_INTERVAL_DELAY = TimeSpan.FromMilliseconds(200);
        private const int RETRY_COUNT = 50;
        private static int _index = 0;
        private const string CONNECTION_NAME_PATTERN = "Event_Source_Producer_{0}";

        /// <summary>
        /// Create REDIS  client.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="intent">The intent.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="endpointKey">The endpoint key.</param>
        /// <param name="passwordKey">The password key.</param>
        /// <returns></returns>
        /// <exception cref="StackExchange.Redis.RedisConnectionException">Fail to establish REDIS connection</exception>
        /// <exception cref="RedisConnectionException">Fail to establish REDIS connection</exception>
        public static async Task<IConnectionMultiplexer> CreateProviderAsync(
                    ILogger logger,
                    RedisUsageIntent intent,
                    Action<ConfigurationOptions>? configuration = null,
                    string endpointKey = CONSUMER_END_POINT_KEY,
                    string passwordKey = CONSUMER_PASSWORD_KEY)
        {
            string endpoint = Environment.GetEnvironmentVariable(endpointKey) ?? "localhost";
            string? password = Environment.GetEnvironmentVariable(passwordKey);

            var sb = new StringBuilder();
            var writer = new StringWriter(sb);

            var redisConfiguration = ConfigurationOptions.Parse(endpoint);
            redisConfiguration.ClientName = string.Format(
                                    CONNECTION_NAME_PATTERN,
                                    Interlocked.Increment(ref _index));
            switch (intent)
            {
                case RedisUsageIntent.Admin:
                    redisConfiguration.AllowAdmin = true;
                    break;
                default:
                    break;
            }
            configuration?.Invoke(redisConfiguration);
            redisConfiguration.Password = password;

            IConnectionMultiplexer redis;
            TimeSpan delay = RETRY_INTERVAL_DELAY;
            for (int i = 1; i <= RETRY_COUNT; i++)
            {
                try
                {
                    redis = await ConnectionMultiplexer.ConnectAsync(redisConfiguration, writer);
                    return redis;
                }
                catch (Exception ex)
                {
                    writer.Flush();
                    if (i >= RETRY_COUNT)
                    {
                        logger.LogError(ex.FormatLazy(), $"Fail to create REDIS client [final retry ({i})]. {Environment.NewLine}{sb}");
                        throw;
                    }
                    if (i % 10 == 0)
                        logger.LogWarning(ex.FormatLazy(), $"Fail to create REDIS client [retry = {i}]. {Environment.NewLine}{sb}");
                }
            }

            throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Fail to establish REDIS connection");
        }


        /// <summary>
        /// Create REDIS database client.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="intent">The intent.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="endpointKey">The endpoint key.</param>
        /// <param name="passwordKey">The password key.</param>
        /// <returns></returns>
        /// <exception cref="StackExchange.Redis.RedisConnectionException">Fail to establish REDIS connection</exception>
        /// <exception cref="RedisConnectionException">Fail to establish REDIS connection</exception>
        public static async Task<IDatabaseAsync> CreateAsync(
                    ILogger logger,
                    RedisUsageIntent intent,
                    Action<ConfigurationOptions>? configuration = null,
                    string endpointKey = CONSUMER_END_POINT_KEY,
                    string passwordKey = CONSUMER_PASSWORD_KEY)
        {
            var provider = await CreateProviderAsync(logger, intent, configuration, endpointKey, passwordKey);
            return provider.GetDatabase();
        }
    }
}
