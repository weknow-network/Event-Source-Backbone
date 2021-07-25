using Microsoft.Extensions.Logging;
using Polly;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Weknow.EventSource.Backbone.Channels.RedisProvider;
using static Weknow.EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;
using Microsoft.Extensions.DependencyInjection;

namespace Weknow.EventSource.Backbone
{
    public static class RedisConsumerProviderExtensions
    {
        /// <summary>
        /// Uses REDIS consumer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="setting">The setting.</param>
        /// <param name="redisConfiguration">The redis configuration.</param>
        /// <param name="endpointEnvKey">The endpoint env key.</param>
        /// <param name="passwordEnvKey">The password env key.</param>
        /// <returns></returns>
        public static IConsumerStoreStrategyBuilder UseRedisChannel(
                            this IConsumerBuilder builder,
                            Func<RedisConsumerChannelSetting, RedisConsumerChannelSetting> setting,
                            Action<ConfigurationOptions>? redisConfiguration = null,
                            string endpointEnvKey = CONSUMER_END_POINT_KEY,
                            string passwordEnvKey = CONSUMER_PASSWORD_KEY)
        {
            var stg = setting?.Invoke(RedisConsumerChannelSetting.Default);
            var channelBuilder = builder.UseChannel(LocalCreate);
            return channelBuilder;

            IConsumerChannelProvider LocalCreate(ILogger logger)
            {
                var channel = new RedisConsumerChannel(
                                        logger,
                                        redisConfiguration,
                                        stg,
                                        endpointEnvKey,
                                        passwordEnvKey);
                return channel;
            }
        }

        /// <summary>
        /// Uses REDIS consumer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="setting">The setting.</param>
        /// <param name="endpointEnvKey">The endpoint env key.</param>
        /// <param name="passwordEnvKey">The password env key.</param>
        /// <returns></returns>
        public static IConsumerStoreStrategyBuilder UseRedisChannel(
                            this IConsumerBuilder builder,
                            RedisConsumerChannelSetting? setting = null,
                            string endpointEnvKey = CONSUMER_END_POINT_KEY,
                            string passwordEnvKey = CONSUMER_PASSWORD_KEY)
        {
            var cfg = setting ?? RedisConsumerChannelSetting.Default;
            var channelBuilder = builder.UseChannel(LocalCreate);
            return channelBuilder;

            IConsumerChannelProvider LocalCreate(ILogger logger)
            {
                var channel = new RedisConsumerChannel(
                                        logger,
                                        setting: cfg,
                                        endpointEnvKey: endpointEnvKey,
                                        passwordEnvKey: passwordEnvKey);
                return channel;
            }
        }

        /// <summary>
        /// Uses REDIS consumer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="redisClient">The redis client.</param>
        /// <param name="setting">The setting.</param>
        /// <returns></returns>
        public static IConsumerStoreStrategyBuilder UseRedisChannel(
                            this IConsumerBuilder builder,
                            Task<IConnectionMultiplexer> redisClient,
                            RedisConsumerChannelSetting? setting = null)
        {
            var cfg = setting ?? RedisConsumerChannelSetting.Default;
            var channelBuilder = builder.UseChannel(LocalCreate);
            return channelBuilder;

            IConsumerChannelProvider LocalCreate(ILogger logger)
            {
                var channel = new RedisConsumerChannel(
                                        redisClient,
                                        logger,
                                        setting);
                return channel;
            }
        }

        /// <summary>
        /// Uses REDIS consumer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="redisClient">The redis client.</param>
        /// <param name="setting">The setting.</param>
        /// <returns></returns>
        public static IConsumerStoreStrategyBuilder UseRedisChannel(
                            this IConsumerBuilder builder,
                            IConnectionMultiplexer redisClient,
                            RedisConsumerChannelSetting? setting = null)
        {
            var cfg = setting ?? RedisConsumerChannelSetting.Default;
            var channelBuilder = builder.UseChannel(LocalCreate);
            return channelBuilder;

            IConsumerChannelProvider LocalCreate(ILogger logger)
            {
                var channel = new RedisConsumerChannel(
                                        redisClient,
                                        logger,
                                        setting);
                return channel;
            }
        }

        /// <summary>
        /// Uses REDIS consumer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="setting">The setting.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">redisClient</exception>
        public static IConsumerStoreStrategyBuilder UseRedisChannelInjection(
                            this IConsumerBuilder builder,
                            IServiceProvider serviceProvider,
                            RedisConsumerChannelSetting? setting = null)
        {
            var redisClient = serviceProvider.GetService<IConnectionMultiplexer>();
            if (redisClient != null)
                return builder.UseRedisChannel(redisClient, setting);
            var redisClientTask = serviceProvider.GetService<Task<IConnectionMultiplexer>>();
            if (redisClientTask == null)
                throw new ArgumentNullException(nameof(redisClientTask));
            return builder.UseRedisChannel(redisClientTask, setting);
        }

    }
}
