using StackExchange.Redis;

using System;

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
        public static IEventSourceConsumerBuilder UseRedisConsumerChannel(
                            this IEventSourceConsumerChannelBuilder builder,
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
        public static IEventSourceProducerOptionsBuilder UseRedisProducerChannel(
                            this IEventSourceProducerChannelBuilder builder,
                            ConfigurationOptions configuration)
        {
            throw new NotImplementedException();
            //return builder.UseChannel(_channel);
        }
    }
}
