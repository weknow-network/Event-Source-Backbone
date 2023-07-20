using System.Collections.Concurrent;
using System.Threading.Channels;

using EventSourcing.Backbone.Building;
using EventSourcing.Backbone.Channels.RedisProvider;
using EventSourcing.Backbone.Tests.Entities;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;



namespace EventSourcing.Backbone.Tests;

public class EndToEndVersionAware_Fallback_Tests: EndToEndVersionAwareBase
{
    private readonly IVersionAwareFallbackConsumer _subscriber = A.Fake<IVersionAwareFallbackConsumer>();

    #region Ctor

    public EndToEndVersionAware_Fallback_Tests(
            ITestOutputHelper outputHelper,
            Func<IProducerStoreStrategyBuilder, ILogger, IProducerStoreStrategyBuilder>? producerChannelBuilder = null,
             Func<IConsumerStoreStrategyBuilder, ILogger, IConsumerStoreStrategyBuilder>? consumerChannelBuilder = null)
            : base(outputHelper, producerChannelBuilder, consumerChannelBuilder)
    {
        A.CallTo(() => _subscriber.Execute_2Async(A<ConsumerMetadata>.Ignored, A<DateTime>.Ignored))
                .ReturnsLazily(() => ValueTask.CompletedTask);
        A.CallTo(() => _subscriber.Execute_4Async(A<ConsumerMetadata>.Ignored, A<TimeSpan>.Ignored))
                .ReturnsLazily(() => ValueTask.CompletedTask);
    }

    #endregion // Ctor

    protected override string Name { get; } = "fallback";

    [Fact]
    public async Task End2End_VersionAware_Fallback_Test()
    {
        IVersionAwareFallbackProducer producer =
            _producerBuilder
                    //.WithOptions(producerOption)
                    .Uri(URI)
                    .WithLogger(TestLogger.Create(_outputHelper))
                    .BuildVersionAwareFallbackProducer();

        var ts = TimeSpan.FromSeconds(1);
        await producer.Execute4Async(ts);
        await producer.Execute1Async(10);
        await producer.Execute1Async(11);

        var cts = new CancellationTokenSource();
        var dic = new ConcurrentDictionary<string, int>();

        var subscription =
             _consumerBuilder
                     .WithOptions(cfg => cfg with { MaxMessages = 3 })
                     .WithCancellation(cts.Token)
                     .Uri(URI)
                     .WithLogger(TestLogger.Create(_outputHelper))
                     .Fallback(ctx =>
                     {
                         Metadata meta = ctx.Metadata;
                         dic.AddOrUpdate($"{meta.Operation}:{meta.Version}", 1, (k,i) => i + 1);
                         ctx.AckAsync(AckBehavior.OnFallback);
                         return Task.CompletedTask;
                     })
                     .SubscribeVersionAwareFallbackConsumer(_subscriber);

        await subscription.Completion;

        A.CallTo(() => _subscriber.Execute_2Async(A<ConsumerMetadata>.Ignored, A<DateTime>.Ignored))
            .MustNotHaveHappened();
        A.CallTo(() => _subscriber.Execute_4Async(A<ConsumerMetadata>.Ignored, ts))
            .MustHaveHappenedOnceExactly();
        Assert.Equal(2, dic["ExecuteAsync:1"]);
    }
}
