using EventSourcing.Backbone.Tests;
using EventSourcing.Backbone.Tests.Entities;

using FakeItEasy;

using Xunit;
using Xunit.Abstractions;

// docker run -p 6379:6379 -it --rm --name redis-evt-src redislabs/rejson:latest

namespace EventSourcing.Backbone.IntegrationTests.HelloWorld
{
    /// <summary>
    /// The end to end tests.
    /// </summary>
    public class HelloWorldTests : TestsBase
    {
        protected override string URI { get; } = $"demo:{DateTimeOffset.Now.ToUnixTimeMilliseconds}";
        private readonly IHelloConsumer _subscriber = A.Fake<IHelloConsumer>();

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="HelloWorldTests"/> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
        public HelloWorldTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        #endregion // Ctor

        #region HelloWorld_Minimal_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task HelloWorld_Minimal_Test()
        {
            IHelloProducer producer = RedisProducerBuilder.Create()
                                            .Environment("testing")
                                            .Uri(URI)
                                            .BuildHelloProducer();

            IConsumerLifetime subscription = RedisConsumerBuilder.Create()
                         .Environment("testing")
                         .Uri(URI)
                         .SubscribeHelloConsumer(_subscriber);

            A.CallTo(() => _subscriber.WorldAsync(A<ConsumerMetadata>.Ignored, 5))
                .Invokes(() => subscription.DisposeAsync());

            await producer.HelloAsync("Hi");
            await producer.WorldAsync(5);


            await subscription.Completion;

            #region Validation

            A.CallTo(() => _subscriber.HelloAsync(A<ConsumerMetadata>.Ignored, "Hi"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.WorldAsync(A<ConsumerMetadata>.Ignored, 5))
                .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // HelloWorld_Minimal_Test

        #region HelloWorld_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task HelloWorld_Test()
        {
            IHelloProducer producer = RedisProducerBuilder.Create()
                                            .Environment("testing")
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildHelloProducer();

            var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            IConsumerLifetime subscription = RedisConsumerBuilder.Create()
                         .WithOptions(o => new ConsumerOptions
                         {
                             MaxMessages = 2, // disconnect after consuming 2 messages
                             AckBehavior = AckBehavior.OnSucceed
                         })
                         .WithCancellation(cancellation.Token)
                         .Environment("testing")
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .Group("CONSUMER_GROUP_1") // the consumer group
                         .Name($"TEST {DateTime.UtcNow:HH:mm:ss}") // the name of the specific consumer
                         .SubscribeHelloConsumer(_subscriber);

            await producer.HelloAsync("Hi");
            await producer.WorldAsync(5);


            await subscription.Completion;

            #region Validation

            A.CallTo(() => _subscriber.HelloAsync(A<ConsumerMetadata>.Ignored, "Hi"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.WorldAsync(A<ConsumerMetadata>.Ignored, 5))
                .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // HelloWorld_Test

        #region HelloWorld_Direct_Endpoint_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task HelloWorld_Direct_Endpoint_Test()
        {
            IHelloProducer producer = RedisProducerBuilder.Create("localhost:6379,localhost:6380")
                                            .Environment("testing")
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildHelloProducer();

            var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            IConsumerLifetime subscription = RedisConsumerBuilder.Create("localhost:6379,localhost:6380")
                         .WithOptions(o => new ConsumerOptions
                         {
                             MaxMessages = 2, // disconnect after consuming 2 messages
                             AckBehavior = AckBehavior.OnSucceed
                         })
                         .WithCancellation(cancellation.Token)
                         .Environment("testing")
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .Group("CONSUMER_GROUP_1") // the consumer group
                         .Name($"TEST {DateTime.UtcNow:HH:mm:ss}") // the name of the specific consumer
                         .SubscribeHelloConsumer(_subscriber);

            await producer.HelloAsync("Hi");
            await producer.WorldAsync(5);


            await subscription.Completion;

            #region Validation

            A.CallTo(() => _subscriber.HelloAsync(A<ConsumerMetadata>.Ignored, "Hi"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.WorldAsync(A<ConsumerMetadata>.Ignored, 5))
                .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // HelloWorld_Direct_Endpoint_Test
    }
}
