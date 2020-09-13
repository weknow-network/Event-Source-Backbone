using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using System;
using System.Threading;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.Channels.RedisProvider;
using Weknow.EventSource.Backbone.Private;

namespace Weknow.EventSource.Backbone
{
    public static class RedisConsumerProviderExtensions
    {
        private const string CONSUMER_END_POINT_KEY = "REDIS_EVENT_SOURCE_CONSUMER_ENDPOINT";
        private const string CONSUMER_PASSWORD_KEY = "REDIS_EVENT_SOURCE_CONSUMER_PASS";

        #region Overloads

        /// <summary>
        /// Uses REDIS consumer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="endpointEnvKey">The endpoint env key.</param>
        /// <param name="passwordEnvKey">The password env key.</param>
        /// <returns></returns>
        public static IConsumerOptionsBuilder UseRedisConsumerChannel(
                            this IConsumerBuilder builder,
                            CancellationToken cancellationToken,
                            Action<ConfigurationOptions>? configuration = null,
                            ILogger? logger = null,
                            string endpointEnvKey = CONSUMER_END_POINT_KEY,
                            string passwordEnvKey = CONSUMER_PASSWORD_KEY)
        {
            return UseRedisConsumerChannel(builder, logger, configuration, cancellationToken, endpointEnvKey, passwordEnvKey);
        }

        #endregion // Overloads

        /// <summary>
        /// Uses REDIS consumer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="endpointEnvKey">The endpoint env key.</param>
        /// <param name="passwordEnvKey">The password env key.</param>
        /// <returns></returns>
        public static IConsumerOptionsBuilder UseRedisConsumerChannel(
                            this IConsumerBuilder builder,
                            ILogger? logger = null,
                            Action<ConfigurationOptions>? configuration = null,
                            CancellationToken cancellationToken = default,
                            string endpointEnvKey = CONSUMER_END_POINT_KEY,
                            string passwordEnvKey = CONSUMER_PASSWORD_KEY)
        {
            var options = ConfigurationOptionsFactory.FromEnv(endpointEnvKey, passwordEnvKey);
            configuration?.Invoke(options);
            var channel = new RedisConsumerChannel(
                                        logger ?? EventSourceFallbakLogger.Default,
                                        options,
                                        endpointEnvKey,
                                        passwordEnvKey);
            cancellationToken.ThrowIfCancellationRequested();
            IConsumerOptionsBuilder result = builder.UseChannel(channel);
            return result;
        }
    }
}
