using FakeItEasy;

using System.Threading.Tasks.Dataflow;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;

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
