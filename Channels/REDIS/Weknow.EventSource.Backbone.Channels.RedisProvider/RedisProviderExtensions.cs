using System;

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    public static class RedisProviderExtensions
    {

        public static IEventSourceConsumerBuilder UsTestConsumerChannel(
                            this IEventSourceConsumerChannelBuilder builder)
        {
            throw new NotImplementedException();
            //return builder.UseChannel(_channel);
        }

        public static IEventSourceProducerBuilder UsTestProducerChannel(
                            this IEventSourceProducerChannelBuilder builder)
        {
            throw new NotImplementedException();
            //return builder.UseChannel(_channel);
        }
    }
}
