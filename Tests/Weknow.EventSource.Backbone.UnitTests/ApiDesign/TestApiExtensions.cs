using System.Collections.Immutable;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{
    public static class TestApiExtensions
    {
        private static readonly Func<ILogger, IConsumerChannelProvider> _consumerChannel = A.Fake<Func<ILogger, IConsumerChannelProvider>>();
        private static readonly Func<ILogger, IProducerChannelProvider> _producerChannelFactory1 = A.Fake<Func<ILogger, IProducerChannelProvider>>();
        private static readonly IProducerChannelProvider _producerChannel1 = A.Fake<IProducerChannelProvider>();
        private static readonly Func<ILogger, IProducerChannelProvider> _producerChannelFactory2 = A.Fake<Func<ILogger, IProducerChannelProvider>>();
        private static readonly IProducerChannelProvider _producerChannel2 = A.Fake<IProducerChannelProvider>();
        private static int _index1 = -1;
        private static int _index2 = -1;

        static TestApiExtensions()
        {
            A.CallTo(() => _producerChannelFactory1.Invoke(A<ILogger>.Ignored)).ReturnsLazily(() => _producerChannel1);
            A.CallTo(() => _producerChannel1.SendAsync(
                                    A<Announcement>.Ignored,
                                    A<ImmutableArray<IProducerStorageStrategyWithFilter>>.Ignored)).
                                    ReturnsLazily(() => $"{(char)(Interlocked.Increment(ref _index1) + 'A')}");
            A.CallTo(() => _producerChannelFactory2.Invoke(A<ILogger>.Ignored)).ReturnsLazily(() => _producerChannel2);
            A.CallTo(() => _producerChannel2.SendAsync(
                                    A<Announcement>.Ignored,
                                    A<ImmutableArray<IProducerStorageStrategyWithFilter>>.Ignored)).
                                    ReturnsLazily(() => $"#{(char)(Interlocked.Increment(ref _index2) + 'a')}");
        }

        public static IConsumerOptionsBuilder UseTestConsumerChannel(
                            this IConsumerBuilder builder)
        {
            return builder.UseChannel(_consumerChannel);
        }

        public static IProducerOptionsBuilder UseTestProducerChannel1(
                            this IProducerBuilder builder)
        {
            return builder.UseChannel(_producerChannelFactory1);
        }

        public static IProducerOptionsBuilder UseTestProducerChannel2(
                            this IProducerBuilder builder)
        {
            return builder.UseChannel(_producerChannelFactory2);
        }
    }
}
