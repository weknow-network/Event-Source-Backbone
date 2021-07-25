using Microsoft.Extensions.Logging;
using Polly;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Weknow.EventSource.Backbone.Channels.RedisProvider;
using Weknow.EventSource.Backbone.Private;

namespace Weknow.EventSource.Backbone
{
    public static class RedisProviderExtensions
    {
        private const string PRODUCER_END_POINT_KEY = "REDIS_EVENT_SOURCE_PRODUCER_ENDPOINT";
        private const string PRODUCER_PASSWORD_KEY = "REDIS_EVENT_SOURCE_PRODUCER_PASS";

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
                            string endpointEnvKey = PRODUCER_END_POINT_KEY,
                            string passwordEnvKey = PRODUCER_PASSWORD_KEY)
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
        /// <param name="redis">The redis database.</param>
        /// <param name="resiliencePolicy">The resilience policy.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IProducerStoreStrategyBuilder UseRedisChannel(
                            this IProducerBuilder builder,
                            Task<IDatabaseAsync> redis,
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
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="resiliencePolicy">The resilience policy.</param>
        /// <returns></returns>
        public static IProducerStoreStrategyBuilder UseRedisChannelInjection(
                            this IProducerBuilder builder,
                            IServiceProvider serviceProvider,
                            AsyncPolicy? resiliencePolicy = null)
        {
            return builder.UseRedisChannel(LocalGetDb(), resiliencePolicy);

            async Task<IDatabaseAsync> LocalGetDb()
            {
                var multiplexer = serviceProvider.GetService<IConnectionMultiplexer>();
                if (multiplexer != null)
                    return multiplexer.GetDatabase();

                var multiplexerTask = serviceProvider.GetService<Task<IConnectionMultiplexer>>();
                if (multiplexerTask == null)
                    throw new ArgumentNullException(nameof(IConnectionMultiplexer));
                var instance = await multiplexerTask;
                IDatabaseAsync db = instance.GetDatabase();
                return db;
            }
        }
    }
}
