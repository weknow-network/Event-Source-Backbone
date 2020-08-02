using FakeItEasy;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{
    public static class TestApiExtensions
    {
        private static readonly IConsumerChannelProvider _consumerChannel = A.Fake<IConsumerChannelProvider>();
        private static readonly IProducerChannelProvider _producerChannel = A.Fake<IProducerChannelProvider>();

        public static IConsumerOptionsBuilder UseTestConsumerChannel(
                            this IConsumerBuilder builder)
        {
            return builder.UseChannel(_consumerChannel);
        }

        public static IProducerOptionsBuilder UseTestProducerChannel(
                            this IProducerBuilder builder)
        {
            return builder.UseChannel(_producerChannel);
        }
    }
}
