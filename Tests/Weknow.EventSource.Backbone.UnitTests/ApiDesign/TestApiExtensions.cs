using System;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{
    public static class TestApiExtensions
    {
        private static readonly Func<ILogger, IConsumerChannelProvider> _consumerChannel = A.Fake<Func<ILogger, IConsumerChannelProvider>>();
        private static readonly Func<ILogger, IProducerChannelProvider> _producerChannel = A.Fake<Func<ILogger, IProducerChannelProvider>>();

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
