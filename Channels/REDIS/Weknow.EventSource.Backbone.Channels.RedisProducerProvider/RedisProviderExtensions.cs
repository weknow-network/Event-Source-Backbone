using Microsoft.Extensions.Logging;
using Polly;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Weknow.EventSource.Backbone.Channels.RedisProvider;
using Weknow.EventSource.Backbone.Private;
using OpenTelemetry.Trace;
using System.Diagnostics;
using static Weknow.EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;
using Microsoft.Extensions.Configuration;

namespace Weknow.EventSource.Backbone
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
        /// <param name="endpointEnvKey">The endpoint env key.</param>
        /// <param name="passwordEnvKey">The password env key.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IProducerStoreStrategyBuilder UseRedisChannel(
                            this IProducerBuilder builder,
                            Action<ConfigurationOptions>? configuration = null,
                            AsyncPolicy? resiliencePolicy = null,
                            string endpointEnvKey = END_POINT_KEY,
                            string passwordEnvKey = PASSWORD_KEY)
        {
            var result = builder.UseChannel(LocalCreate);
            return result;

            IProducerChannelProvider LocalCreate(ILogger logger)
            {
                var channel = new RedisProducerChannel(
                                 logger ?? EventSourceFallbakLogger.Default,
                                 configuration,
                                 resiliencePolicy,
                                 endpointEnvKey,
                                 passwordEnvKey);
                return channel;
            }
        }

        /// <summary>
        /// Uses REDIS producer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="redis">The redis database.</param>
        /// <param name="resiliencePolicy">The resilience policy.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IProducerStoreStrategyBuilder UseRedisChannel(
                            this IProducerBuilder builder,
                            IDatabaseAsync redis,
                            AsyncPolicy? resiliencePolicy = null)
        {
            var result = builder.UseChannel(LocalCreate);
            return result;

            IProducerChannelProvider LocalCreate(ILogger logger)
            {
                var channel = new RedisProducerChannel(
                                 redis,
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
