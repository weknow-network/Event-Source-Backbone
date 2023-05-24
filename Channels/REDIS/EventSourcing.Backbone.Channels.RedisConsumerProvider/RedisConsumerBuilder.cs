using EventSourcing.Backbone.Building;
using EventSourcing.Backbone.Channels.RedisProvider;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace EventSourcing.Backbone
{
    public static class RedisConsumerBuilder
    {
        /// <summary>
        /// Create REDIS consumer builder.
        /// </summary>
        /// <param name="endpoint">The raw endpoint (not an environment variable).</param>
        /// <param name="password">The password (not an environment variable).</param>
        /// <param name="configurationHook">The configuration hook.</param>
        /// <returns></returns>
        public static IConsumerStoreStrategyBuilder Create(
                            string endpoint,
                            string? password = null,
                            Action<ConfigurationOptions>? configurationHook = null)
        {
            var configuration = RedisClientFactory.CreateConfigurationOptions(endpoint, password, configurationHook);
            return configuration.CreateRedisConsumerBuilder();
        }
        /// <summary>
        /// Create REDIS consumer builder.
        /// </summary>
        /// <param name="configurationHook">The configuration hook.</param>
        /// <returns></returns>
        public static IConsumerStoreStrategyBuilder Create(
                            Action<ConfigurationOptions>? configurationHook = null)
        {
            var configuration = RedisClientFactory.CreateConfigurationOptions(configurationHook);
            return configuration.CreateRedisConsumerBuilder();
        }

        /// <summary>
        /// Create REDIS consumer builder.
        /// </summary>
        /// <param name="options">The redis configuration.</param>
        /// <param name="setting">The setting.</param>
        /// <returns></returns>
        public static IConsumerStoreStrategyBuilder CreateRedisConsumerBuilder(
                            this ConfigurationOptions options,
                            RedisConsumerChannelSetting? setting = null)
        {
            var stg = setting ?? RedisConsumerChannelSetting.Default;
            var builder = ConsumerBuilder.Empty;
            var channelBuilder = builder.UseChannel(LocalCreate);
            return channelBuilder;

            IConsumerChannelProvider LocalCreate(ILogger logger)
            {
                var channel = new RedisConsumerChannel(
                                        logger,
                                        options,
                                        stg);
                return channel;
            }
        }

        /// <summary>
        /// Create REDIS consumer builder.
        /// </summary>
        /// <param name="setting">The setting.</param>
        /// <param name="configurationHook">The configuration hook.</param>
        /// <returns></returns>
        public static IConsumerStoreStrategyBuilder CreateRedisConsumerBuilder(
                            this RedisConsumerChannelSetting setting,
                            Action<ConfigurationOptions>? configurationHook = null)
        {
            var configuration = RedisClientFactory.CreateConfigurationOptions(configurationHook);
            return configuration.CreateRedisConsumerBuilder(setting);
        }

        /// <summary>
        /// Create REDIS consumer builder.
        /// </summary>
        /// <param name="setting">The setting.</param>
        /// <param name="endpoint">The raw endpoint (not an environment variable).</param>
        /// <param name="password">The password (not an environment variable).</param>
        /// <param name="configurationHook">The configuration hook.</param>
        /// <returns></returns>
        public static IConsumerStoreStrategyBuilder CreateRedisConsumerBuilder(
                            this RedisConsumerChannelSetting setting,
                            string endpoint,
                            string? password = null,
                            Action<ConfigurationOptions>? configurationHook = null)
        {
            var configuration = RedisClientFactory.CreateConfigurationOptions(endpoint, password, configurationHook);
            return configuration.CreateRedisConsumerBuilder(setting);
        }


        /// <summary>
        /// Create REDIS consumer builder.
        /// </summary>
        /// <param name="credentialsKeys">The credentials keys.</param>
        /// <param name="setting">The setting.</param>
        /// <param name="configurationHook">The configuration hook.</param>
        /// <returns></returns>
        public static IConsumerStoreStrategyBuilder CreateRedisConsumerBuilder(
                            this RedisCredentialsEnvKeys credentialsKeys,
                            RedisConsumerChannelSetting? setting = null,
                            Action<ConfigurationOptions>? configurationHook = null)
        {
            var configuration = credentialsKeys.CreateConfigurationOptions(configurationHook);
            return configuration.CreateRedisConsumerBuilder(setting);
        }

        /// <summary>
        /// Uses REDIS consumer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="setting">The setting.</param>
        /// <param name="redisConfiguration">The redis configuration.</param>
        /// <returns></returns>
        public static IConsumerStoreStrategyBuilder UseRedisChannel(
                            this IConsumerBuilder builder,
                            RedisConsumerChannelSetting? setting = null,
                            ConfigurationOptions? redisConfiguration = null)
        {
            var stg = setting ?? RedisConsumerChannelSetting.Default;
            var channelBuilder = builder.UseChannel(LocalCreate);
            return channelBuilder;

            IConsumerChannelProvider LocalCreate(ILogger logger)
            {
                var channel = new RedisConsumerChannel(
                                        logger,
                                        redisConfiguration,
                                        stg);
                return channel;
            }
        }

        /// <summary>
        /// Uses REDIS consumer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="credentialsKeys">Environment keys of the credentials</param>
        /// <param name="setting">The setting.</param>
        /// <returns></returns>
        public static IConsumerStoreStrategyBuilder UseRedisChannel(
                            this IConsumerBuilder builder,
                            RedisCredentialsEnvKeys credentialsKeys,
                            RedisConsumerChannelSetting? setting = null)
        {
            var channelBuilder = builder.UseChannel(LocalCreate);
            return channelBuilder;

            IConsumerChannelProvider LocalCreate(ILogger logger)
            {
                var channel = new RedisConsumerChannel(
                                        logger,
                                        credentialsKeys,
                                        setting);
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
        internal static IConsumerStoreStrategyBuilder UseRedisChannel(
                            this IConsumerBuilder builder,
                            IEventSourceRedisConnectionFacroty redisClientFactory,
                            RedisConsumerChannelSetting? setting = null)
        {
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

        /// <summary>
        /// Uses REDIS consumer channel.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="setting">The setting.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">redisClient</exception>
        public static IConsumerStoreStrategyBuilder UseRedisChannelInjection(
                            this IServiceProvider serviceProvider,
                            RedisConsumerChannelSetting? setting = null)
        {
            var connFactory = serviceProvider.GetService<IEventSourceRedisConnectionFacroty>();
            if (connFactory == null)
                throw new RedisConnectionException(ConnectionFailureType.None, $"{nameof(IEventSourceRedisConnectionFacroty)} is not registerd, use services.{nameof(RedisDiExtensions.AddEventSourceRedisConnection)} in order to register it at Setup stage.");

            var builder = ConsumerBuilder.Empty;
            return builder.UseRedisChannel(connFactory, setting);
        }

    }
}
