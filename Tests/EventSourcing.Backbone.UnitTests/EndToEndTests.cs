using System.Threading;
using System.Threading.Channels;

using EventSourcing.Backbone.Enums;
using EventSourcing.Backbone.UnitTests.Entities;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;



namespace EventSourcing.Backbone
{
    public class EndToEndTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IProducerBuilder _producerBuilder = ProducerBuilder.Empty;
        private readonly IConsumerBuilder _consumerBuilder = ConsumerBuilder.Empty;
        private readonly Func<ILogger, IProducerChannelProvider> _producerChannel;
        private readonly Func<ILogger, IConsumerChannelProvider> _consumerChannel;
        private readonly Channel<Announcement> ch;
        private readonly ISequenceOfConsumer _subscriber1 = A.Fake<ISequenceOfConsumer>();
        private readonly ISequenceOfConsumer _subscriber2 = A.Fake<ISequenceOfConsumer>();
        private readonly ISequenceOfConsumer _subscriber3 = A.Fake<ISequenceOfConsumer>();

        private readonly ISimpleEventConsumer _simpleEventConsumer = A.Fake<ISimpleEventConsumer>();
        private readonly ISubscriptionBridge _simpleBridgeSubscription;
        private readonly ISubscriptionBridge _simpleGenSubscription;
        private readonly ISubscriptionBridge _simpleGenBridgeSubscription;

        protected const int TIMEOUT = 1_000 * 4;

        #region Ctor

        public EndToEndTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            ch = Channel.CreateUnbounded<Announcement>();
            _producerChannel = _ => new ProducerTestChannel(ch);
            _consumerChannel = _ => new ConsumerTestChannel(ch);
            _simpleBridgeSubscription = new SimpleEventSubscriptionBridge(_simpleEventConsumer);
            _simpleGenSubscription = new SimpleEventSubscriptionFromGen(_simpleEventConsumer);
            _simpleGenBridgeSubscription = new SimpleEventConsumerBridge(_simpleEventConsumer);
        }

        #endregion // Ctor

        #region End2End_CustomBaseSubscription_Test

        [Fact]
        public async Task End2End_CustomBaseSubscription_Test()
        {
            ISimpleEventProducer producer =
                _producerBuilder.UseChannel(_producerChannel)
                        //.WithOptions(producerOption)
                        .Uri("Kids#HappySocks")
                        .WithLogger(TestLogger.Create(_outputHelper))
                        .BuildSimpleEventProducer();

            await producer.ExecuteAsync("Id", 1);
            await producer.RunAsync(1, DateTime.Now);
            await producer.RunAsync(2, DateTime.Now);

            var cts = new CancellationTokenSource();
            IAsyncDisposable subscription =
                 _consumerBuilder.UseChannel(_consumerChannel)
                         //.WithOptions(consumerOptions)
                         .WithCancellation(cts.Token)
                         .Uri("Kids#HappySocks")
                         .WithLogger(TestLogger.Create(_outputHelper))
                         .SubscribeSimpleEvent(_simpleEventConsumer);

            ch.Writer.Complete();
            await subscription.DisposeAsync();
            await ch.Reader.Completion;

            A.CallTo(() => _simpleEventConsumer.ExecuteAsync(A<ConsumerMetadata>.Ignored, "Id", 1))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _simpleEventConsumer.RunAsync(A<ConsumerMetadata>.Ignored, 1, A<DateTime>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _simpleEventConsumer.RunAsync(A<ConsumerMetadata>.Ignored, 2, A<DateTime>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        #endregion // End2End_CustomBaseSubscription_Test

        #region End2End_CustomSubscriptionBridge_Test

        [Fact]
        public async Task End2End_CustomSubscriptionBridge_Test()
        {
            ISimpleEventProducer producer =
                _producerBuilder.UseChannel(_producerChannel)
                        //.WithOptions(producerOption)
                        .Uri("Kids#HappySocks")
                        .BuildSimpleEventProducer();

            await producer.ExecuteAsync("Id", 1);
            await producer.RunAsync(1, DateTime.Now);
            await producer.RunAsync(2, DateTime.Now);

            var cts = new CancellationTokenSource();
            IAsyncDisposable subscription =
                 _consumerBuilder.UseChannel(_consumerChannel)
                         //.WithOptions(consumerOptions)
                         .WithCancellation(cts.Token)
                         .Uri("Kids#HappySocks")
                         .Subscribe(_simpleBridgeSubscription.BridgeAsync);

            ch.Writer.Complete();
            await subscription.DisposeAsync();
            await ch.Reader.Completion;

            A.CallTo(() => _simpleEventConsumer.ExecuteAsync(A<ConsumerMetadata>.Ignored, "Id", 1))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _simpleEventConsumer.RunAsync(A<ConsumerMetadata>.Ignored, 1, A<DateTime>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _simpleEventConsumer.RunAsync(A<ConsumerMetadata>.Ignored, 2, A<DateTime>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        #endregion // End2End_CustomSubscriptionBridge_Test

        #region End2End_GenBaseSubscription_Test

        [Fact]
        public async Task End2End_GenBaseSubscription_Test()
        {
            ISimpleEventProducer producer =
                _producerBuilder.UseChannel(_producerChannel)
                        //.WithOptions(producerOption)
                        .Uri("Kids#HappySocks")
                        .BuildSimpleEventProducer();

            await producer.ExecuteAsync("Id", 1);
            await producer.RunAsync(1, DateTime.Now);
            await producer.RunAsync(2, DateTime.Now);

            var cts = new CancellationTokenSource();
            IAsyncDisposable subscription =
                 _consumerBuilder.UseChannel(_consumerChannel)
                         //.WithOptions(consumerOptions)
                         .WithCancellation(cts.Token)
                         .Uri("Kids#HappySocks")
                         .Subscribe(_simpleGenSubscription);

            ch.Writer.Complete();
            await subscription.DisposeAsync();
            await ch.Reader.Completion;

            A.CallTo(() => _simpleEventConsumer.ExecuteAsync(A<ConsumerMetadata>.Ignored, "Id", 1))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _simpleEventConsumer.RunAsync(A<ConsumerMetadata>.Ignored, 1, A<DateTime>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _simpleEventConsumer.RunAsync(A<ConsumerMetadata>.Ignored, 2, A<DateTime>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        #endregion // End2End_GenBaseSubscription_Test

        #region End2End_GenSubscriptionBridge_Test

        [Fact]
        public async Task End2End_GenSubscriptionBridge_Test()
        {
            ISimpleEventProducer producer =
                _producerBuilder.UseChannel(_producerChannel)
                        //.WithOptions(producerOption)
                        .Uri("Kids#HappySocks")
                        .BuildSimpleEventProducer();

            await producer.ExecuteAsync("Id", 1);
            await producer.RunAsync(1, DateTime.Now);
            await producer.RunAsync(2, DateTime.Now);

            var cts = new CancellationTokenSource();
            IAsyncDisposable subscription =
                 _consumerBuilder.UseChannel(_consumerChannel)
                         //.WithOptions(consumerOptions)
                         .WithCancellation(cts.Token)
                         .Uri("Kids#HappySocks")
                         .Subscribe(_simpleGenBridgeSubscription.BridgeAsync);

            ch.Writer.Complete();
            await subscription.DisposeAsync();
            await ch.Reader.Completion;

            A.CallTo(() => _simpleEventConsumer.ExecuteAsync(A<ConsumerMetadata>.Ignored, "Id", 1))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _simpleEventConsumer.RunAsync(A<ConsumerMetadata>.Ignored, 1, A<DateTime>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _simpleEventConsumer.RunAsync(A<ConsumerMetadata>.Ignored, 2, A<DateTime>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        #endregion // End2End_GenSubscriptionBridge_Test

        #region End2End_Test

        [Theory(Timeout = TIMEOUT)]
        [InlineData(MultiConsumerBehavior.All)]
        [InlineData(MultiConsumerBehavior.Once)]
        public async Task End2End_Test(MultiConsumerBehavior multiConsumerBehavior)
        {
            ISequenceOperationsProducer producer =
                _producerBuilder.UseChannel(_producerChannel)
                        //.WithOptions(producerOption)
                        .Uri("Kids#HappySocks")
                        .WithLogger(TestLogger.Create(_outputHelper))
                        .BuildSequenceOperationsProducer();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);

            var cts = new CancellationTokenSource();
            IAsyncDisposable subscription =
                 _consumerBuilder.UseChannel(_consumerChannel)
                         //.WithOptions(consumerOptions)
                         .WithOptions(c => c with { MultiConsumerBehavior = multiConsumerBehavior })
                         .WithCancellation(cts.Token)
                         .Uri("Kids#HappySocks")
                         .WithLogger(TestLogger.Create(_outputHelper))
                         .Subscribe(new SequenceOfConsumerBridge(_subscriber1));

            ch.Writer.Complete();
            await subscription.DisposeAsync();
            await ch.Reader.Completion;

            A.CallTo(() => _subscriber1.RegisterAsync(A<ConsumerMetadata>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber1.LoginAsync(A<ConsumerMetadata>.Ignored, "admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber1.EarseAsync(A<ConsumerMetadata>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();
        }

        #endregion // End2End_Test

        #region End2End_MultiTargets_Test

        [Theory(Timeout = TIMEOUT)]
        [InlineData(MultiConsumerBehavior.All)]
        [InlineData(MultiConsumerBehavior.Once)]
        public async Task End2End_MultiTargets_Test(MultiConsumerBehavior multiConsumerBehavior)
        {
            ISequenceOperationsProducer producer =
                _producerBuilder.UseChannel(_producerChannel)
                        //.WithOptions(producerOption)
                        .Uri("Kids#HappySocks")
                        .WithLogger(TestLogger.Create(_outputHelper))
                        .BuildSequenceOperationsProducer();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);

            var cts = new CancellationTokenSource();
            IAsyncDisposable subscription =
                 _consumerBuilder.UseChannel(_consumerChannel)
                         //.WithOptions(consumerOptions)
                         .WithOptions(c => c with { MultiConsumerBehavior = multiConsumerBehavior })
                         .WithCancellation(cts.Token)
                         .Uri("Kids#HappySocks")
                         .WithLogger(TestLogger.Create(_outputHelper))
                         .SubscribeSequenceOfConsumer(_subscriber1, _subscriber2, _subscriber3);

            ch.Writer.Complete();
            await Task.Delay(500);
            await subscription.DisposeAsync();
            await ch.Reader.Completion;

            A.CallTo(() => _subscriber1.RegisterAsync(A<ConsumerMetadata>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber1.LoginAsync(A<ConsumerMetadata>.Ignored, "admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber1.EarseAsync(A<ConsumerMetadata>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _subscriber2.RegisterAsync(A<ConsumerMetadata>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber2.LoginAsync(A<ConsumerMetadata>.Ignored, "admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber2.EarseAsync(A<ConsumerMetadata>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _subscriber3.RegisterAsync(A<ConsumerMetadata>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber3.LoginAsync(A<ConsumerMetadata>.Ignored, "admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber3.EarseAsync(A<ConsumerMetadata>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();
        }

        #endregion // End2End_MultiTargets_Test

        #region End2End_MultiSubscribersTargets_Test

        [Theory(Skip = "Testing bug' deadlock", Timeout = TIMEOUT)]
        //[Theory(Timeout = TIMEOUT)]
        [InlineData(MultiConsumerBehavior.All)]
        [InlineData(MultiConsumerBehavior.Once)]
        public async Task End2End_MultiSubscribersTargets_Test(MultiConsumerBehavior multiConsumerBehavior)
        {
            ISequenceOperationsProducer producer =
                _producerBuilder.UseChannel(_producerChannel)
                        //.WithOptions(producerOption)
                        .Uri("Kids#HappySocks")
                        .WithLogger(TestLogger.Create(_outputHelper))
                        .BuildSequenceOperationsProducer();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);

            var cts = new CancellationTokenSource();
            IAsyncDisposable subscription =
                 _consumerBuilder.UseChannel(_consumerChannel)
                         //.WithOptions(consumerOptions)
                         .WithOptions(c => c with { MultiConsumerBehavior = multiConsumerBehavior })
                         .WithCancellation(cts.Token)
                         .Uri("Kids#HappySocks")
                         .WithLogger(TestLogger.Create(_outputHelper))
                         .Subscribe(new SequenceOfConsumerBridge(_subscriber1))
                         .SubscribeSequenceOfConsumer(_subscriber2, _subscriber3);

            ch.Writer.Complete();
            await Task.Delay(3000);
            await subscription.DisposeAsync();
            await ch.Reader.Completion;

            A.CallTo(() => _subscriber1.RegisterAsync(A<ConsumerMetadata>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber1.LoginAsync(A<ConsumerMetadata>.Ignored, "admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber1.EarseAsync(A<ConsumerMetadata>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _subscriber2.RegisterAsync(A<ConsumerMetadata>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber2.LoginAsync(A<ConsumerMetadata>.Ignored, "admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber2.EarseAsync(A<ConsumerMetadata>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();

            if (multiConsumerBehavior == MultiConsumerBehavior.All)
            {
                A.CallTo(() => _subscriber3.RegisterAsync(A<ConsumerMetadata>.Ignored, A<User>.Ignored))
                    .MustHaveHappenedOnceExactly();
                A.CallTo(() => _subscriber3.LoginAsync(A<ConsumerMetadata>.Ignored, "admin", "1234"))
                    .MustHaveHappenedOnceExactly();
                A.CallTo(() => _subscriber3.EarseAsync(A<ConsumerMetadata>.Ignored, 4335))
                    .MustHaveHappenedOnceExactly();
            }
            else
            {
                A.CallTo(() => _subscriber3.RegisterAsync(A<ConsumerMetadata>.Ignored, A<User>.Ignored))
                    .MustNotHaveHappened();
                A.CallTo(() => _subscriber3.LoginAsync(A<ConsumerMetadata>.Ignored, "admin", "1234"))
                    .MustNotHaveHappened();
                A.CallTo(() => _subscriber3.EarseAsync(A<ConsumerMetadata>.Ignored, 4335))
                    .MustNotHaveHappened();
            }
        }

        #endregion // End2End_MultiSubscribersTargets_Test

        #region End2End_Overloads_Test

        [Fact]
        public async Task End2End_Overloads_Test()
        {
            ISimpleEventProducer producer =
                _producerBuilder.UseChannel(_producerChannel)
                        //.WithOptions(producerOption)
                        .Uri("Kids#HappySocks")
                        .WithLogger(TestLogger.Create(_outputHelper))
                        .BuildSimpleEventProducer();

            await producer.RunAsync(42, DateTime.Now);
            await producer.RunAsync(42);
            await producer.RunAsync(TimeSpan.FromSeconds(42));

            var cts = new CancellationTokenSource();
            IAsyncDisposable subscription =
                 _consumerBuilder.UseChannel(_consumerChannel)
                         //.WithOptions(consumerOptions)
                         .WithCancellation(cts.Token)
                         .Uri("Kids#HappySocks")
                         .WithLogger(TestLogger.Create(_outputHelper))
                         .SubscribeSimpleEventConsumer(_simpleEventConsumer);

            ch.Writer.Complete();
            await subscription.DisposeAsync();
            await ch.Reader.Completion;

            A.CallTo(() => _simpleEventConsumer.RunAsync(A<ConsumerMetadata>.Ignored, 42))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _simpleEventConsumer.RunAsync(A<ConsumerMetadata>.Ignored, TimeSpan.FromSeconds(42)))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _simpleEventConsumer.RunAsync(A<ConsumerMetadata>.Ignored, 42, A<DateTime>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        #endregion // End2End_Overloads_Test
    }
}
