using EventSourcing.Backbone.Building;
using EventSourcing.Backbone.Tests.Entities;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace EventSourcing.Backbone.Tests;

public class EndToEndVersionAware_DerivedFallback_Tests : EndToEndVersionAwareBase
{
    private readonly IVersionAwareDerivedFallbackConsumer _subscriber = A.Fake<IVersionAwareDerivedFallbackConsumer>();

    #region Ctor

    public EndToEndVersionAware_DerivedFallback_Tests(
            ITestOutputHelper outputHelper,
            Func<IProducerStoreStrategyBuilder, ILogger, IProducerStoreStrategyBuilder>? producerChannelBuilder = null,
             Func<IConsumerStoreStrategyBuilder, ILogger, IConsumerStoreStrategyBuilder>? consumerChannelBuilder = null)
            : base(outputHelper, producerChannelBuilder, consumerChannelBuilder)
    {
        A.CallTo(() => _subscriber.Execute_2Async(A<ConsumerContext>.Ignored, A<DateTime>.Ignored))
                .ReturnsLazily(() => ValueTask.CompletedTask);
    }

    #endregion // Ctor

    protected override string Name { get; } = "fallback-derived";

    [Fact]
    public async Task End2End_VersionAware_DerivedFallback_Test()
    {
        IVersionAwareDerivedFallbackProducer producer =
            _producerBuilder
                    .Uri(URI)
                    .WithLogger(TestLogger.Create(_outputHelper))
                    .BuildVersionAwareDerivedFallbackProducer();

        var ts = TimeSpan.FromSeconds(1);
        await producer.Execute4Async(ts);
        await producer.Execute1Async(10);
        await producer.Execute2Async(DateTime.Now);
        await producer.Execute1Async(11);

        var cts = new CancellationTokenSource();

        var subscription =
             _consumerBuilder
                     .WithOptions(cfg => cfg with { MaxMessages = 4 })
                     .WithCancellation(cts.Token)
                     .Uri(URI)
                     .WithLogger(TestLogger.Create(_outputHelper))
                     .SubscribeVersionAwareDerivedFallbackConsumer(_subscriber);

        await subscription.Completion;

        A.CallTo(() => _subscriber.Execute_2Async(A<ConsumerContext>.Ignored, A<DateTime>.Ignored))
            .MustHaveHappened();
        A.CallTo(() => _subscriber.Execute_3Async(A<ConsumerContext>.Ignored, "10"))
            .MustHaveHappened();
        A.CallTo(() => _subscriber.Execute_3Async(A<ConsumerContext>.Ignored, "11"))
            .MustHaveHappened();
        A.CallTo(() => _subscriber.Execute_3Async(A<ConsumerContext>.Ignored, "00:00:01"))
            .MustHaveHappened();
    }
}
