using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using System;
using System.Threading;

using Weknow.EventSource.Backbone.Channels.RedisProvider;
using Weknow.EventSource.Backbone.Private;

namespace Weknow.EventSource.Backbone
{
    public static class RedisProviderExtensions
    {
        private const string PRODUCER_END_POINT_KEY = "REDIS_EVENT_SOURCE_PRODUCER_ENDPOINT";
        private const string PRODUCER_PASSWORD_KEY = "REDIS_EVENT_SOURCE_PRODUCER_PASS";

        #region Overloads

        /// <summary>
        /// Uses REDIS producer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="endpointEnvKey">The endpoint env key.</param>
        /// <param name="passwordEnvKey">The password env key.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IProducerOptionsBuilder UseRedisProducerChannel(
                            this IProducerBuilder builder,
                            CancellationToken cancellationToken,
                            ILogger? logger = null,
                            Action<ConfigurationOptions, CancellationToken>? configuration = null,
                            string endpointEnvKey = PRODUCER_END_POINT_KEY,
                            string passwordEnvKey = PRODUCER_PASSWORD_KEY)
        {
            return UseRedisProducerChannel(builder, logger, configuration, cancellationToken, endpointEnvKey, passwordEnvKey);
        }

        #endregion // Overloads

        /// <summary>
        /// Uses REDIS producer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="endpointEnvKey">The endpoint env key.</param>
        /// <param name="passwordEnvKey">The password env key.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IProducerOptionsBuilder UseRedisProducerChannel(
                            this IProducerBuilder builder,
                            ILogger? logger = null,
                            Action<ConfigurationOptions, CancellationToken>? configuration = null,
                            CancellationToken cancellationToken = default,
                            string endpointEnvKey = PRODUCER_END_POINT_KEY,
                            string passwordEnvKey = PRODUCER_PASSWORD_KEY)
        {
            var options = ConfigurationOptionsFactory.FromEnv(endpointEnvKey, passwordEnvKey);
            configuration?.Invoke(options, cancellationToken);
            var channel = new RedisProducerChannel(
                                        logger ?? EventSourceFallbakLogger.Default,
                                        options,
                                        endpointEnvKey,
                                        passwordEnvKey);
            cancellationToken.ThrowIfCancellationRequested();
            IProducerOptionsBuilder result = builder.UseChannel(channel);
            return result;
        }
    }
}
