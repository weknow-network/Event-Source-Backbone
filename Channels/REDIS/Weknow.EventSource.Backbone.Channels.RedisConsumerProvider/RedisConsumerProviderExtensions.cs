using Microsoft.Extensions.Logging;
using Polly;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Weknow.EventSource.Backbone.Channels.RedisProvider;
using static Weknow.EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Configuration;
using Weknow.EventSource.Backbone.Building;

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
        /// <param name="credentialsKeys">Environment keys of the credentials</param>
        /// <returns></returns>
        public static IConsumerStoreStrategyBuilder UseRedisChannel(
                            this IConsumerBuilder builder,
                            Func<RedisConsumerChannelSetting, RedisConsumerChannelSetting> setting,
                            Action<ConfigurationOptions>? redisConfiguration = null,
                            RedisCredentialsKeys credentialsKeys = default)
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
                                        credentialsKeys);
                return channel;
            }
        }

        /// <summary>
        /// Uses REDIS consumer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="setting">The setting.</param>
        /// <param name="credentialsKeys">Environment keys of the credentials</param>
        /// <returns></returns>
        public static IConsumerStoreStrategyBuilder UseRedisChannel(
                            this IConsumerBuilder builder,
                            RedisConsumerChannelSetting? setting = null,
                        RedisCredentialsKeys credentialsKeys = default)
        {
            var cfg = setting ?? RedisConsumerChannelSetting.Default;
            var channelBuilder = builder.UseChannel(LocalCreate);
            return channelBuilder;

            IConsumerChannelProvider LocalCreate(ILogger logger)
            {
                var channel = new RedisConsumerChannel(
                                        logger,
                                        setting: cfg,
                                        credentialsKeys: credentialsKeys);
                return channel;
            }
        }

        /// <summary>
        /// Uses REDIS consumer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="redisClientFactory">The redis client factory.</param>
        /// <param name="setting">The setting.</param>
        /// <returns></returns>
        public static IConsumerStoreStrategyBuilder UseRedisChannel(
                            this IConsumerBuilder builder,
                            IEventSourceRedisConnectionFacroty redisClientFactory,
                            RedisConsumerChannelSetting? setting = null)
        {
            var cfg = setting ?? RedisConsumerChannelSetting.Default;
            var channelBuilder = builder.UseChannel(LocalCreate);
            return channelBuilder;

            IConsumerChannelProvider LocalCreate(ILogger logger)
            {
                var channel = new RedisConsumerChannel(
                                        redisClientFactory,
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
            var connFactory = serviceProvider.GetService<IEventSourceRedisConnectionFacroty>();
            if (connFactory == null)
                throw new RedisConnectionException(ConnectionFailureType.None, $"{nameof(IEventSourceRedisConnectionFacroty)} is not registerd, use services.{nameof(RedisDiExtensions.AddEventSourceRedisConnection)} in order to register it at Setup stage.");
            return builder.UseRedisChannel(connFactory, setting);
        }

    }
}
