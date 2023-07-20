using System.Threading.Channels;

using EventSourcing.Backbone.Building;
using EventSourcing.Backbone.Tests.Entities;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;



namespace EventSourcing.Backbone.Tests;

public class EndToEndVersionAware_NamingAppend_Tests: EndToEndVersionAwareBase
{
    private readonly IVersionAwareAppendConsumer _subscriber = A.Fake<IVersionAwareAppendConsumer>();

    #region Ctor

    public EndToEndVersionAware_NamingAppend_Tests(
            ITestOutputHelper outputHelper,
            Func<IProducerStoreStrategyBuilder, ILogger, IProducerStoreStrategyBuilder>? producerChannelBuilder = null,
             Func<IConsumerStoreStrategyBuilder, ILogger, IConsumerStoreStrategyBuilder>? consumerChannelBuilder = null)
            : base(outputHelper, producerChannelBuilder, consumerChannelBuilder)
    {
        A.CallTo(() => _subscriber.Execute2Async(A<ConsumerMetadata>.Ignored, A<DateTime>.Ignored))
               .ReturnsLazily(() => ValueTask.CompletedTask);
        A.CallTo(() => _subscriber.Execute1Async(A<ConsumerMetadata>.Ignored, A<int>.Ignored))
                .ReturnsLazily(() => ValueTask.CompletedTask);
        A.CallTo(() => _subscriber.Execute4Async(A<ConsumerMetadata>.Ignored, A<TimeSpan>.Ignored))
                .ReturnsLazily(() => ValueTask.CompletedTask);
    }

    #endregion // Ctor

    protected override string Name { get; } = "append";

    [Fact]
    public async Task End2End_VersionAware_Append_Test()
    {
        IVersionAwareAppendProducer producer =
            _producerBuilder
                    //.WithOptions(producerOption)
                    .Uri(URI)
                    .WithLogger(TestLogger.Create(_outputHelper))
                    .BuildVersionAwareAppendProducer();

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
                     .SubscribeVersionAwareAppendConsumer(_subscriber);

        await subscription.Completion;

        A.CallTo(() => _subscriber.Execute2Async(A<ConsumerMetadata>.Ignored, A<DateTime>.Ignored))
            .MustNotHaveHappened();
        A.CallTo(() => _subscriber.Execute1Async(A<ConsumerMetadata>.Ignored, 10))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _subscriber.Execute1Async(A<ConsumerMetadata>.Ignored, 11))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _subscriber.Execute4Async(A<ConsumerMetadata>.Ignored, ts))
            .MustHaveHappenedOnceExactly();
    }
}
