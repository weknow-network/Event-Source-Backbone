using EventSourcing.Backbone.Building;
using EventSourcing.Backbone.Tests.Entities;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;



namespace EventSourcing.Backbone.Tests;

public class EndToEndVersionAware_Fallback_Tests : EndToEndVersionAwareBase
{
    private readonly IVersionAwareFallbackConsumer _subscriber = A.Fake<IVersionAwareFallbackConsumer>();

    #region Ctor

    public EndToEndVersionAware_Fallback_Tests(
            ITestOutputHelper outputHelper,
            Func<IProducerStoreStrategyBuilder, ILogger, IProducerStoreStrategyBuilder>? producerChannelBuilder = null,
             Func<IConsumerStoreStrategyBuilder, ILogger, IConsumerStoreStrategyBuilder>? consumerChannelBuilder = null)
            : base(outputHelper, producerChannelBuilder, consumerChannelBuilder)
    {
        A.CallTo(() => _subscriber.Execute_2Async(A<ConsumerContext>.Ignored, A<DateTime>.Ignored))
                .ReturnsLazily(() => ValueTask.CompletedTask);
        A.CallTo(() => _subscriber.Execute_4Async(A<ConsumerContext>.Ignored, A<TimeSpan>.Ignored))
                .ReturnsLazily(() => ValueTask.CompletedTask);
    }

    #endregion // Ctor

    protected override string Name { get; } = "fallback";

    [Fact(Skip = "fallback implementation is missing")]
    public async Task End2End_VersionAware_Fallback_Test()
    {
        IVersionAwareFallbackProducer producer =
            _producerBuilder
                    .Uri(URI)
                    .WithLogger(TestLogger.Create(_outputHelper))
                    .BuildVersionAwareFallbackProducer();

        var ts = TimeSpan.FromSeconds(1);
        await producer.Execute4Async(ts);
        await producer.Execute1Async(10);
        await producer.Execute1Async(11);

        var cts = new CancellationTokenSource();

        var subscription =
             _consumerBuilder
                     .WithOptions(cfg => cfg with { MaxMessages = 3 })
                     .WithCancellation(cts.Token)
                     .Uri(URI)
                     .WithLogger(TestLogger.Create(_outputHelper))
                     // TODO: fallback
                     .SubscribeVersionAwareFallbackConsumer(_subscriber);

        await subscription.Completion;

        A.CallTo(() => _subscriber.Execute_2Async(A<ConsumerContext>.Ignored, A<DateTime>.Ignored))
            .MustNotHaveHappened();
        A.CallTo(() => _subscriber.Execute_4Async(A<ConsumerContext>.Ignored, ts))
            .MustHaveHappenedOnceExactly();

        throw new NotImplementedException();
    }
}
