using FakeItEasy;

using System.Threading.Tasks.Dataflow;

using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;

namespace Weknow.EventSource.Backbone
{
    public static class TestApiExtensions
    {
        private static readonly IConsumerChannelProvider _consumerChannel = A.Fake<IConsumerChannelProvider>();
        private static readonly IProducerChannelProvider _producerChannel = A.Fake<IProducerChannelProvider>();

        public static IEventSourceConsumerBuilder UsTestConsumerChannel(
                            this IEventSourceConsumerChannelBuilder builder)
        {
            return builder.UseChannel(_consumerChannel);
        }

        public static IEventSourceProducerBuilder UsTestProducerChannel(
                            this IEventSourceProducerChannelBuilder builder)
        {
            return builder.UseChannel(_producerChannel);
        }
    }
}
