using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    /// <summary>
    /// REDIS client factory
    /// </summary>
    public class RedisClientFactory : IRedisClientFactory
    {
        private static readonly TimeSpan RETRY_INTERVAL_DELAY = TimeSpan.FromMilliseconds(200);
        private const int RETRY_COUNT = 50;
        private readonly Task<ConnectionMultiplexer> _multiplexerTask;
        private Task<IDatabaseAsync>? _dbTask;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisClientFactory" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="name">Identification for the connection within REDIS.</param>
        /// <param name="intent">The usage intent.</param>
        /// <param name="endpointKey">The environment key of endpoint.</param>
        /// <param name="passwordKey">The environment key of password.</param>
        public RedisClientFactory(
                    ILogger logger,
                    string name,
                    RedisUsageIntent intent,
                    string endpointKey,
                    string passwordKey)
        {
            _multiplexerTask = CreateAsync(
                                        logger,
                                        name,
                                        intent,
                                        endpointKey,
                                        passwordKey);
        }

        #endregion // Ctor

        #region CreateAsync

        /// <summary>
        /// Create REDIS client.
        /// </summary>
        /// <returns></returns>
        public Task<ConnectionMultiplexer> CreateAsync() => _multiplexerTask;

        /// <summary>
        /// Create REDIS client.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="name">The name.</param>
        /// <param name="intent">The intent.</param>
        /// <param name="endpointKey">The endpoint key.</param>
        /// <param name="passwordKey">The password key.</param>
        /// <returns></returns>
        private async Task<ConnectionMultiplexer> CreateAsync(
                    ILogger logger,
                    string name,
                    RedisUsageIntent intent,
                    string endpointKey,
                    string passwordKey)
        {
            string endpoint = Environment.GetEnvironmentVariable(endpointKey) ?? "localhost";
            string? password = Environment.GetEnvironmentVariable(passwordKey);

            var sb = new StringBuilder();
            var writer = new StringWriter(sb);

            var redisConfiguration = ConfigurationOptions.Parse(endpoint);
            redisConfiguration.ClientName = name;
            redisConfiguration.Password = password;
            if(!Debugger.IsAttached)
                redisConfiguration.ServiceName = "mymaster";
            switch (intent)
            {
                case RedisUsageIntent.Admin:
                    redisConfiguration.AllowAdmin = true;
                    break;
                default:
                    break;
            }

            ConnectionMultiplexer redis;
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
                    if(i % 10 == 0)
                        logger.LogWarning(ex.FormatLazy(), $"Fail to create REDIS client [retry = {i}]. {Environment.NewLine}{sb}");
                }
            }

            throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Fail to establish REDIS connection");
        }

        #endregion // CreateAsync

        #region CreateDbAsync

        /// <summary>
        /// Creates the database asynchronous.
        /// </summary>
        /// <returns></returns>
        public Task<IDatabaseAsync> GetDbAsync()
        {
            _dbTask = _dbTask ?? GetDatabaseAsync();
            return _dbTask;

            async Task<IDatabaseAsync> GetDatabaseAsync()
            {
                ConnectionMultiplexer redis = await _multiplexerTask;
                IDatabaseAsync db = redis.GetDatabase();
                return db;
            }
        }

        #endregion // CreateDbAsync

        //#region GetSubscriberAsync

        ///// <summary>
        ///// Gets the subscriber.
        ///// </summary>
        ///// <returns></returns>
        //public async Task<ISubscriber> GetSubscriberAsync()
        //{
        //    ConnectionMultiplexer redis = await _multiplexerTask;
        //    ISubscriber sub = redis.GetSubscriber();
        //    return sub;
        //}

        //#endregion // GetSubscriberAsync
    }
}
