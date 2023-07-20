using System.Threading.Channels;

using EventSourcing.Backbone.Building;
using EventSourcing.Backbone.Tests.Entities;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;



namespace EventSourcing.Backbone.Tests;

public class EndToEndVersionAware_NamingNone_Tests: EndToEndVersionAwareBase
{
    private readonly IVersionAwareNoneConsumer _subscriber = A.Fake<IVersionAwareNoneConsumer>();

    #region Ctor

    public EndToEndVersionAware_NamingNone_Tests(
            ITestOutputHelper outputHelper,
            Func<IProducerStoreStrategyBuilder, ILogger, IProducerStoreStrategyBuilder>? producerChannelBuilder = null,
             Func<IConsumerStoreStrategyBuilder, ILogger, IConsumerStoreStrategyBuilder>? consumerChannelBuilder = null)
            : base(outputHelper, producerChannelBuilder, consumerChannelBuilder)
    {
        A.CallTo(() => _subscriber.ExecuteAsync(A<ConsumerMetadata>.Ignored, A<DateTime>.Ignored))
                .ReturnsLazily(() => ValueTask.CompletedTask);
        A.CallTo(() => _subscriber.ExecuteAsync(A<ConsumerMetadata>.Ignored, A<int>.Ignored))
                .ReturnsLazily(() => ValueTask.CompletedTask);
        A.CallTo(() => _subscriber.ExecuteAsync(A<ConsumerMetadata>.Ignored, A<TimeSpan>.Ignored))
                .ReturnsLazily(() => ValueTask.CompletedTask);
    }

    #endregion // Ctor

    protected override string Name { get; } = "none";

    [Fact]
    public async Task End2End_VersionAware_None_Test()
    {
        IVersionAwareNoneProducer producer =
            _producerBuilder
                    //.WithOptions(producerOption)
                    .Uri(URI)
                    .WithLogger(TestLogger.Create(_outputHelper))
                    .BuildVersionAwareNoneProducer();

        var ts = TimeSpan.FromSeconds(1);
        await producer.ExecuteAsync(ts);
        await producer.ExecuteAsync(10);
        await producer.ExecuteAsync(11);

        CancellationToken cancellation = GetCancellationToken(3);
        var subscription =
             _consumerBuilder
                     .WithOptions(cfg => cfg with { MaxMessages = 3 })
                     .WithCancellation(cancellation)
                     .Uri(URI)
                     .WithLogger(TestLogger.Create(_outputHelper))
                     .SubscribeVersionAwareNoneConsumer(_subscriber);

        await subscription.Completion;

        A.CallTo(() => _subscriber.ExecuteAsync(A<ConsumerMetadata>.Ignored, A<DateTime>.Ignored))
            .MustNotHaveHappened();
        A.CallTo(() => _subscriber.ExecuteAsync(A<ConsumerMetadata>.Ignored, 10))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _subscriber.ExecuteAsync(A<ConsumerMetadata>.Ignored, 11))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _subscriber.ExecuteAsync(A<ConsumerMetadata>.Ignored, ts))
            .MustHaveHappenedOnceExactly();
    }
}
