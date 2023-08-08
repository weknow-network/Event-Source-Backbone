using System.Threading.Channels;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;



namespace EventSourcing.Backbone.UnitTests;

public class EndToEndVersionAware_Fallback_Tests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IProducerBuilder _producerBuilder = ProducerBuilder.Empty;
    private readonly IConsumerBuilder _consumerBuilder = ConsumerBuilder.Empty;
    private readonly Func<ILogger, IProducerChannelProvider> _producerChannel;
    private readonly Func<ILogger, IConsumerChannelProvider> _consumerChannel;
    private readonly Channel<Announcement> ch;
    private readonly IVersionAwareFallbackConsumer _subscriber = A.Fake<IVersionAwareFallbackConsumer>();

    #region Ctor

    public EndToEndVersionAware_Fallback_Tests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        ch = Channel.CreateUnbounded<Announcement>();
        _producerChannel = _ => new ProducerTestChannel(ch);
        _consumerChannel = _ => new ConsumerTestChannel(ch);
    }

    #endregion // Ctor

    [Fact(Skip = "fallback implementation is missing")]
    public async Task End2End_VersionAware_Fallback_Test()
    {
        string URI = "testing:version:aware";
        IVersionAwareFallbackProducer producer =
            _producerBuilder.UseChannel(_producerChannel)
                    //.WithOptions(producerOption)
                    .Uri(URI)
                    .WithLogger(TestLogger.Create(_outputHelper))
                    .BuildVersionAwareFallbackProducer();

        var ts = TimeSpan.FromSeconds(1);
        await producer.Execute4Async(ts);
        await producer.Execute1Async(10);
        await producer.Execute1Async(11);
        await producer.Execute2Async(DateTime.Now);

        var cts = new CancellationTokenSource();

        IAsyncDisposable subscription =
             _consumerBuilder.UseChannel(_consumerChannel)
                     //.WithOptions(consumerOptions)
                     .WithCancellation(cts.Token)
                     .Uri(URI)
                     .WithLogger(TestLogger.Create(_outputHelper))
                     .SubscribeVersionAwareFallbackConsumer(_subscriber);

        ch.Writer.Complete();
        await subscription.DisposeAsync();
        await ch.Reader.Completion;

        A.CallTo(() => _subscriber.Execute_2Async(A<ConsumerContext>.Ignored, A<DateTime>.Ignored))
            .MustHaveHappened();
        A.CallTo(() => _subscriber.Execute_3Async(A<ConsumerContext>.Ignored, "1"))
            .MustHaveHappened();
        A.CallTo(() => _subscriber.Execute_3Async(A<ConsumerContext>.Ignored, "10"))
            .MustHaveHappened();
        A.CallTo(() => _subscriber.Execute_3Async(A<ConsumerContext>.Ignored, "11"))
            .MustHaveHappened();

        throw new NotImplementedException();
    }
}
