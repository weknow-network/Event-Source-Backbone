using System.Threading.Channels;

using EventSourcing.Backbone.UnitTests.Entities;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;



namespace EventSourcing.Backbone.UnitTests;

public class EndToEndVersionAware_NamingAppendUnderscore_Tests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IProducerBuilder _producerBuilder = ProducerBuilder.Empty;
    private readonly IConsumerBuilder _consumerBuilder = ConsumerBuilder.Empty;
    private readonly Func<ILogger, IProducerChannelProvider> _producerChannel;
    private readonly Func<ILogger, IConsumerChannelProvider> _consumerChannel;
    private readonly Channel<Announcement> ch;
    private readonly IVersionAwareAppendUnderscoreConsumer _subscriber = A.Fake<IVersionAwareAppendUnderscoreConsumer>();

    #region Ctor

    public EndToEndVersionAware_NamingAppendUnderscore_Tests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        ch = Channel.CreateUnbounded<Announcement>();
        _producerChannel = _ => new ProducerTestChannel(ch);
        _consumerChannel = _ => new ConsumerTestChannel(ch);
    }

    #endregion // Ctor

    [Fact]
    public async Task End2End_VersionAware_AppendUnderscore_Test()
    {
        string URI = "testing:version:aware";
        IVersionAwareAppendUnderscoreProducer producer =
            _producerBuilder.UseChannel(_producerChannel)
                    //.WithOptions(producerOption)
                    .Uri(URI)
                    .WithLogger(TestLogger.Create(_outputHelper))
                    .BuildVersionAwareAppendUnderscoreProducer();

        var ts = TimeSpan.FromSeconds(1);
        await producer.Execute_4Async(ts);
        await producer.Execute_1Async(10);
        await producer.Execute_1Async(11);

        var cts = new CancellationTokenSource();
        IAsyncDisposable subscription =
             _consumerBuilder.UseChannel(_consumerChannel)
                     //.WithOptions(consumerOptions)
                     .WithCancellation(cts.Token)
                     .Uri(URI)
                     .WithLogger(TestLogger.Create(_outputHelper))
                     .SubscribeVersionAwareAppendUnderscoreConsumer(_subscriber);

        ch.Writer.Complete();
        await subscription.DisposeAsync();
        await ch.Reader.Completion;

        A.CallTo(() => _subscriber.Execute_2Async(A<ConsumerContext>.Ignored, A<DateTime>.Ignored))
            .MustNotHaveHappened();
        A.CallTo(() => _subscriber.Execute_1Async(A<ConsumerContext>.Ignored, 10))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _subscriber.Execute_1Async(A<ConsumerContext>.Ignored, 11))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _subscriber.Execute_4Async(A<ConsumerContext>.Ignored, ts))
            .MustHaveHappenedOnceExactly();
    }
}
