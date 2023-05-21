using EventSourcing.Backbone.Channels.RedisProvider;
using EventSourcing.Backbone.Private;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OpenTelemetry.Trace;

using Polly;

using StackExchange.Redis;

namespace EventSourcing.Backbone
{
    public static class RedisProviderExtensions
    {
        /// <summary>
        /// Adds the event producer telemetry source (will result in tracing the producer).
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static TracerProviderBuilder AddEventProducerTelemetry(this TracerProviderBuilder builder) => builder.AddSource(nameof(RedisProducerChannel));

        /// <summary>
        /// Uses REDIS producer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="resiliencePolicy">The resilience policy.</param>
        /// <param name="credentialsKeys">Environment keys of the credentials</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IProducerStoreStrategyBuilder UseRedisChannel(
                            this IProducerBuilder builder,
                            Action<ConfigurationOptions>? configuration = null,
                            AsyncPolicy? resiliencePolicy = null,
                            RedisCredentialsKeys credentialsKeys = default)
        {
            var result = builder.UseChannel(LocalCreate);
            return result;

            IProducerChannelProvider LocalCreate(ILogger logger)
            {
                var connFactory = new EventSourceRedisConnectionFacroty(logger, configuration, credentialsKeys);
                var channel = new RedisProducerChannel(
                                    connFactory,
                                    logger ?? EventSourceFallbakLogger.Default,
                                    resiliencePolicy);
                return channel;
            }
        }

        /// <summary>
        /// Uses REDIS producer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="redisConnectionFactory">The redis database.</param>
        /// <param name="resiliencePolicy">The resilience policy.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IProducerStoreStrategyBuilder UseRedisChannel(
                            this IProducerBuilder builder,
                            IEventSourceRedisConnectionFacroty redisConnectionFactory,
                            AsyncPolicy? resiliencePolicy = null)
        {
            var result = builder.UseChannel(LocalCreate);
            return result;

            IProducerChannelProvider LocalCreate(ILogger logger)
            {
                var channel = new RedisProducerChannel(
                                 redisConnectionFactory,
                                 logger ?? EventSourceFallbakLogger.Default,
                                 resiliencePolicy);
                return channel;
            }
        }

        /// <summary>
        /// Uses REDIS producer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="resiliencePolicy">The resilience policy.</param>
        /// <returns></returns>
        public static IProducerStoreStrategyBuilder UseRedisChannelInjection(
                            this IProducerBuilder builder,
                            IServiceProvider serviceProvider,
                            AsyncPolicy? resiliencePolicy = null)
        {
            ILogger logger = serviceProvider.GetService<ILogger<IProducerBuilder>>() ?? throw new ArgumentNullException();

            var connFactory = serviceProvider.GetService<IEventSourceRedisConnectionFacroty>();
            if (connFactory == null)
                throw new RedisConnectionException(ConnectionFailureType.None, $"{nameof(IEventSourceRedisConnectionFacroty)} is not registerd, use services.{nameof(RedisDiExtensions.AddEventSourceRedisConnection)} in order to register it at Setup stage.");
            return builder.UseRedisChannel(connFactory, resiliencePolicy);
        }
    }
}
