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

        /// <summary>
        /// Uses REDIS consumer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="redisConfiguration">The redis configuration.</param>
        /// <param name="policy">The policy.</param>
        /// <param name="claimingTrigger">The claiming trigger.</param>
        /// <param name="endpointEnvKey">The endpoint env key.</param>
        /// <param name="passwordEnvKey">The password env key.</param>
        /// <returns></returns>
        public static IConsumerOptionsBuilder UseRedisConsumerChannel(
                            this IConsumerBuilder builder,
                            CancellationToken cancellationToken,
                            ILogger? logger = null,
                            Action<ConfigurationOptions>? redisConfiguration = null,
                            ResiliencePolicies? policy = null,
                            StaleMessagesClaimingTrigger? claimingTrigger = null,
                            string endpointEnvKey = CONSUMER_END_POINT_KEY,
                            string passwordEnvKey = CONSUMER_PASSWORD_KEY)
        {
            var setting = new RedisConsumerChannelSetting
            {
                ClaimingTrigger = claimingTrigger ?? StaleMessagesClaimingTrigger.Empty,
                Policy = policy ?? ResiliencePolicies.Empty,
                RedisConfiguration = redisConfiguration
            };

            return UseRedisConsumerChannel(builder, cancellationToken, setting,
                                            logger, endpointEnvKey, passwordEnvKey);
        }

        /// <summary>
        /// Uses REDIS consumer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="setting">The setting.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="endpointEnvKey">The endpoint env key.</param>
        /// <param name="passwordEnvKey">The password env key.</param>
        /// <returns></returns>
        public static IConsumerOptionsBuilder UseRedisConsumerChannel(
                            this IConsumerBuilder builder,
                            CancellationToken cancellationToken,
                            RedisConsumerChannelSetting? setting = null,
                            ILogger? logger = null,
                            string endpointEnvKey = CONSUMER_END_POINT_KEY,
                            string passwordEnvKey = CONSUMER_PASSWORD_KEY)
        {
            var options = ConfigurationOptionsFactory.FromEnv(endpointEnvKey, passwordEnvKey);
            var cfg = setting ?? RedisConsumerChannelSetting.Empty;
            cfg.RedisConfiguration?.Invoke(options);
            var channel = new RedisConsumerChannel(
                                        logger ?? EventSourceFallbakLogger.Default,
                                        options,
                                        cfg,
                                        endpointEnvKey,
                                        passwordEnvKey);
            cancellationToken.ThrowIfCancellationRequested();
            IConsumerOptionsBuilder result = builder.UseChannel(channel);
            return result;
        }
    }
}
