using System.Threading.Channels;

using EventSourcing.Backbone.Building;
using EventSourcing.Backbone.UnitTests.Entities;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;



namespace EventSourcing.Backbone
{
    public class StoreStrategyTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IProducerBuilder _producerBuilder = ProducerBuilder.Empty;
        private readonly IConsumerBuilder _consumerBuilder = ConsumerBuilder.Empty;
        private readonly Func<ILogger, IProducerChannelProvider> _producerChannel;
        private readonly Func<ILogger, IConsumerChannelProvider> _consumerChannel;
        // private readonly IDataSerializer _serializer;
        private readonly IProducerInterceptor _rawInterceptor = A.Fake<IProducerInterceptor>();
        private readonly IProducerAsyncInterceptor _rawAsyncInterceptor = A.Fake<IProducerAsyncInterceptor>();
        private readonly IProducerAsyncSegmentationStrategy _segmentationStrategy = A.Fake<IProducerAsyncSegmentationStrategy>();
        private readonly IProducerSegmentationStrategy _otherSegmentationStrategy = A.Fake<IProducerSegmentationStrategy>();
        private readonly IProducerSegmentationStrategy _postSegmentationStrategy = A.Fake<IProducerSegmentationStrategy>();
        private readonly Channel<Announcement> _ch;
        private readonly ISequenceOfConsumer _subscriber = A.Fake<ISequenceOfConsumer>();
        private readonly IProducerStorageStrategy _producerStorageStrategyA = A.Fake<IProducerStorageStrategy>();
        private readonly IProducerStorageStrategy _producerStorageStrategyB = A.Fake<IProducerStorageStrategy>();
        private readonly IConsumerStorageStrategy _consumerStorageStrategyA = A.Fake<IConsumerStorageStrategy>();
        private readonly IConsumerStorageStrategy _consumerStorageStrategyB = A.Fake<IConsumerStorageStrategy>();
        private readonly IConsumerStorageStrategy _consumerStorageStrategyC = A.Fake<IConsumerStorageStrategy>();

        #region Ctor

        public StoreStrategyTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            // _serializer = new JsonDataSerializer();
            _ch = Channel.CreateUnbounded<Announcement>();
            _producerChannel = _ => new ProducerTestChannel(_ch);
            _consumerChannel = _ => new ConsumerTestChannel(_ch);
        }

        #endregion // Ctor

        #region Build_Serializer_Producer_Test

        [Fact]
        public async Task Build_Serializer_Producer_Test()
        {
            bool LocalOnlyEmail(string key) => key == "email";
            bool LocalAllButEmail(string key) => !LocalOnlyEmail(key);

            ISequenceOperationsProducer producer =
                _producerBuilder.UseChannel(_producerChannel)
                        .AddStorageStrategy(l => _producerStorageStrategyA.ToValueTask(), filter: LocalOnlyEmail)
                        .AddStorageStrategy(l => _producerStorageStrategyB.ToValueTask(), filter: LocalAllButEmail)
                        //.WithOptions(producerOption)
                        .Uri("Kids:HappySocks")
                        .BuildSequenceOperationsProducer();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);

            //var consumerOptions = new EventSourceConsumerOptions(serializer: _serializer);

            var cts = new CancellationTokenSource();
            IAsyncDisposable subscription =
                 _consumerBuilder.UseChannel(_consumerChannel)
                         .AddStorageStrategyFactory(l => _consumerStorageStrategyA.ToValueTask())
                         .AddStorageStrategyFactory(l => _consumerStorageStrategyB.ToValueTask(), EventBucketCategories.Segments)
                         .AddStorageStrategyFactory(l => _consumerStorageStrategyC.ToValueTask(), EventBucketCategories.Interceptions)
                         //.WithOptions(consumerOptions)
                         .WithCancellation(cts.Token)
                         .Uri("Kids:HappySocks")
                         .Subscribe(new SequenceOfConsumerBridge(_subscriber));

            _ch.Writer.Complete();
            await subscription.DisposeAsync();
            await _ch.Reader.Completion;

            A.CallTo(() => _subscriber.RegisterAsync(A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.LoginAsync("admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.EarseAsync(4335))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _producerStorageStrategyA.SaveBucketAsync(
                                                        A<string>.Ignored,
                                                        A<Bucket>.That.Matches(m => m.ContainsKey("email")),
                                                        A<EventBucketCategories>.Ignored,
                                                        A<Metadata>.Ignored,
                                                        A<CancellationToken>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _producerStorageStrategyB.SaveBucketAsync(
                                                        A<string>.Ignored,
                                                        A<Bucket>.That.Matches(m => m.ContainsKey("email")),
                                                        A<EventBucketCategories>.Ignored,
                                                        A<Metadata>.Ignored,
                                                        A<CancellationToken>.Ignored))
                .MustNotHaveHappened();
            A.CallTo(() => _consumerStorageStrategyA.LoadBucketAsync(
                                                        A<Metadata>.Ignored,
                                                        A<Bucket>.Ignored,
                                                        EventBucketCategories.Segments,
                                                        A<Func<string, string>>.Ignored,
                                                        A<CancellationToken>.Ignored))
                .MustHaveHappenedOnceOrMore();
            A.CallTo(() => _consumerStorageStrategyA.LoadBucketAsync(
                                                        A<Metadata>.Ignored,
                                                        A<Bucket>.Ignored,
                                                        EventBucketCategories.Interceptions,
                                                        A<Func<string, string>>.Ignored,
                                                        A<CancellationToken>.Ignored))
                .MustHaveHappenedOnceOrMore();
            A.CallTo(() => _consumerStorageStrategyB.LoadBucketAsync(
                                                        A<Metadata>.Ignored,
                                                        A<Bucket>.Ignored,
                                                        EventBucketCategories.Segments,
                                                        A<Func<string, string>>.Ignored,
                                                        A<CancellationToken>.Ignored))
                .MustHaveHappenedOnceOrMore();
            A.CallTo(() => _consumerStorageStrategyB.LoadBucketAsync(
                                                        A<Metadata>.Ignored,
                                                        A<Bucket>.Ignored,
                                                        EventBucketCategories.Interceptions,
                                                        A<Func<string, string>>.Ignored,
                                                        A<CancellationToken>.Ignored))
                .MustNotHaveHappened();
            A.CallTo(() => _consumerStorageStrategyC.LoadBucketAsync(
                                                        A<Metadata>.Ignored,
                                                        A<Bucket>.Ignored,
                                                        EventBucketCategories.Interceptions,
                                                        A<Func<string, string>>.Ignored,
                                                        A<CancellationToken>.Ignored))
                .MustHaveHappenedOnceOrMore();
            A.CallTo(() => _consumerStorageStrategyC.LoadBucketAsync(
                                                        A<Metadata>.Ignored,
                                                        A<Bucket>.Ignored,
                                                        EventBucketCategories.Segments,
                                                        A<Func<string, string>>.Ignored,
                                                        A<CancellationToken>.Ignored))
                .MustNotHaveHappened();
        }

        #endregion // Build_Serializer_Producer_Test
    }
}
