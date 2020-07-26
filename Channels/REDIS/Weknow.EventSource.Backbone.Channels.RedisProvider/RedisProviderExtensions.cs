using StackExchange.Redis;

using System;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    public static class RedisProviderExtensions
    {

        /// <summary>
        /// Uses REDIS consumer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IConsumerOptionsBuilder UseRedisConsumerChannel(
                            this IConsumerBuilder builder,
                            ConfigurationOptions configuration)
        {
            throw new NotImplementedException();
            //return builder.UseChannel(_channel);
        }

        /// <summary>
        /// Uses REDIS producer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IProducerOptionsBuilder UseRedisProducerChannel(
                            this IProducerBuilder builder,
                            ConfigurationOptions configuration)
        {
            throw new NotImplementedException();
            //return builder.UseChannel(_channel);
        }
    }
}
