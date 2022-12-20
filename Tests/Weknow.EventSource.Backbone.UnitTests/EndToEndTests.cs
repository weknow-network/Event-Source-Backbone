using System.Threading.Channels;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;



namespace Weknow.EventSource.Backbone
{
    public class EndToEndTests
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
        private readonly Channel<Announcement> ch;
        private readonly ISequenceOfConsumer _subscriber = A.Fake<ISequenceOfConsumer>();

        private readonly ISimpleEventConsumer _simpleEventConsumer = A.Fake<ISimpleEventConsumer>();
        private readonly ISubscriptionBridge _simpleInheritSubscription;
        private readonly ISubscriptionBridge _simpleBridgeSubscription;
        private readonly ISubscriptionBridge _simpleGenSubscription;
        private readonly ISubscriptionBridge _simpleGenBridgeSubscription;

        #region Ctor

        public EndToEndTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            // _serializer = new JsonDataSerializer();
            ch = Channel.CreateUnbounded<Announcement>();
            _producerChannel = _ => new ProducerTestChannel(ch);
            _consumerChannel = _ => new ConsumerTestChannel(ch);
            _simpleInheritSubscription = new SimpleEventSubscription(_simpleEventConsumer);
            _simpleBridgeSubscription = new SimpleEventSubscriptionBridge(_simpleEventConsumer);
            _simpleGenSubscription = new SimpleEventSubscriptionFromGen(_simpleEventConsumer);
            _simpleGenBridgeSubscription = new SimpleEventConsumerBridge(_simpleEventConsumer);
        }

        #endregion // Ctor

        #region End2End_CustomBaseSubscription_Test

        [Fact]
        public async Task End2End_CustomBaseSubscription_Test()
        {
            //var producerOption = new EventSourceOptions(_serializer);

            ISimpleEventProducer producer =
                _producerBuilder.UseChannel(_producerChannel)
                        //.WithOptions(producerOption)
                        .Partition("Organizations")
                        .Shard("Org: #RedSocks")
                        .BuildSimpleEventProducer();

            await producer.ExecuteAsync("Id", 1);
            await producer.RunAsync(1, DateTime.Now);
            await producer.RunAsync(2, DateTime.Now);

            //var consumerOptions = new EventSourceConsumerOptions(serializer: _serializer);

            var cts = new CancellationTokenSource();
            IAsyncDisposable subscription =
                 _consumerBuilder.UseChannel(_consumerChannel)
                         //.WithOptions(consumerOptions)
                         .WithCancellation(cts.Token)
                         .Partition("Organizations")
                         .Shard("Org: #RedSocks")
                         .SubscribeSimpleEvent(_simpleEventConsumer);

            ch.Writer.Complete();
            await subscription.DisposeAsync();
            await ch.Reader.Completion;

            A.CallTo(() => _simpleEventConsumer.ExecuteAsync("Id", 1))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _simpleEventConsumer.RunAsync(1, A<DateTime>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _simpleEventConsumer.RunAsync(2, A<DateTime>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        #endregion // End2End_CustomBaseSubscription_Test

        #region End2End_CustomSubscriptionBridge_Test

        [Fact]
        public async Task End2End_CustomSubscriptionBridge_Test()
        {
            //var producerOption = new EventSourceOptions(_serializer);

            ISimpleEventProducer producer =
                _producerBuilder.UseChannel(_producerChannel)
                        //.WithOptions(producerOption)
                        .Partition("Organizations")
                        .Shard("Org: #RedSocks")
                        .BuildSimpleEventProducer();

            await producer.ExecuteAsync("Id", 1);
            await producer.RunAsync(1, DateTime.Now);
            await producer.RunAsync(2, DateTime.Now);

            //var consumerOptions = new EventSourceConsumerOptions(serializer: _serializer);

            var cts = new CancellationTokenSource();
            IAsyncDisposable subscription =
                 _consumerBuilder.UseChannel(_consumerChannel)
                         //.WithOptions(consumerOptions)
                         .WithCancellation(cts.Token)
                         .Partition("Organizations")
                         .Shard("Org: #RedSocks")
                         .Subscribe(_simpleBridgeSubscription.BridgeAsync);

            ch.Writer.Complete();
            await subscription.DisposeAsync();
            await ch.Reader.Completion;

            A.CallTo(() => _simpleEventConsumer.ExecuteAsync("Id", 1))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _simpleEventConsumer.RunAsync(1, A<DateTime>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _simpleEventConsumer.RunAsync(2, A<DateTime>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        #endregion // End2End_CustomSubscriptionBridge_Test

        #region End2End_GenBaseSubscription_Test

        [Fact]
        public async Task End2End_GenBaseSubscription_Test()
        {
            //var producerOption = new EventSourceOptions(_serializer);

            ISimpleEventProducer producer =
                _producerBuilder.UseChannel(_producerChannel)
                        //.WithOptions(producerOption)
                        .Partition("Organizations")
                        .Shard("Org: #RedSocks")
                        .BuildSimpleEventProducer();

            await producer.ExecuteAsync("Id", 1);
            await producer.RunAsync(1, DateTime.Now);
            await producer.RunAsync(2, DateTime.Now);

            //var consumerOptions = new EventSourceConsumerOptions(serializer: _serializer);

            var cts = new CancellationTokenSource();
            IAsyncDisposable subscription =
                 _consumerBuilder.UseChannel(_consumerChannel)
                         //.WithOptions(consumerOptions)
                         .WithCancellation(cts.Token)
                         .Partition("Organizations")
                         .Shard("Org: #RedSocks")
                         .Subscribe(_simpleGenSubscription);

            ch.Writer.Complete();
            await subscription.DisposeAsync();
            await ch.Reader.Completion;

            A.CallTo(() => _simpleEventConsumer.ExecuteAsync("Id", 1))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _simpleEventConsumer.RunAsync(1, A<DateTime>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _simpleEventConsumer.RunAsync(2, A<DateTime>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        #endregion // End2End_GenBaseSubscription_Test

        #region End2End_GenSubscriptionBridge_Test

        [Fact]
        public async Task End2End_GenSubscriptionBridge_Test()
        {
            //var producerOption = new EventSourceOptions(_serializer);

            ISimpleEventProducer producer =
                _producerBuilder.UseChannel(_producerChannel)
                        //.WithOptions(producerOption)
                        .Partition("Organizations")
                        .Shard("Org: #RedSocks")
                        .BuildSimpleEventProducer();

            await producer.ExecuteAsync("Id", 1);
            await producer.RunAsync(1, DateTime.Now);
            await producer.RunAsync(2, DateTime.Now);

            //var consumerOptions = new EventSourceConsumerOptions(serializer: _serializer);

            var cts = new CancellationTokenSource();
            IAsyncDisposable subscription =
                 _consumerBuilder.UseChannel(_consumerChannel)
                         //.WithOptions(consumerOptions)
                         .WithCancellation(cts.Token)
                         .Partition("Organizations")
                         .Shard("Org: #RedSocks")
                         .Subscribe(_simpleGenBridgeSubscription.BridgeAsync);

            ch.Writer.Complete();
            await subscription.DisposeAsync();
            await ch.Reader.Completion;

            A.CallTo(() => _simpleEventConsumer.ExecuteAsync("Id", 1))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _simpleEventConsumer.RunAsync(1, A<DateTime>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _simpleEventConsumer.RunAsync(2, A<DateTime>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        #endregion // End2End_GenSubscriptionBridge_Test

        #region End2End_Test

        [Fact]
        public async Task End2End_Test()
        {
            //var producerOption = new EventSourceOptions(_serializer);

            ISequenceOperationsProducer producer =
                _producerBuilder.UseChannel(_producerChannel)
                        //.WithOptions(producerOption)
                        .Partition("Organizations")
                        .Shard("Org: #RedSocks")
                        .BuildSequenceOperationsProducer();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);

            //var consumerOptions = new EventSourceConsumerOptions(serializer: _serializer);

            var cts = new CancellationTokenSource();
            IAsyncDisposable subscription =
                 _consumerBuilder.UseChannel(_consumerChannel)
                         //.WithOptions(consumerOptions)
                         .WithCancellation(cts.Token)
                         .Partition("Organizations")
                         .Shard("Org: #RedSocks")
                         .Subscribe(new SequenceOfConsumerBridge(_subscriber));

            ch.Writer.Complete();
            await subscription.DisposeAsync();
            await ch.Reader.Completion;

            A.CallTo(() => _subscriber.RegisterAsync(A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.LoginAsync("admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.EarseAsync(4335))
                .MustHaveHappenedOnceExactly();
        }

        #endregion // End2End_Test
    }
}
