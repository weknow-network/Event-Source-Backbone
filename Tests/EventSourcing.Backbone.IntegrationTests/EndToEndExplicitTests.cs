using System.Diagnostics;
using System.Text.Json;

using EventSourcing.Backbone.Building;
using EventSourcing.Backbone.UnitTests.Entities;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using Xunit;
using Xunit.Abstractions;

#pragma warning disable S3881 // "IDisposable" should be implemented correctly

// docker run -p 6379:6379 -it --rm --name redis-event-source redislabs/rejson:latest


// TODO: [bnaya 2020-10] ensure message order(cancel ack should cancel all following messages)
// TODO: [bnaya 2020-10] check for no pending

namespace EventSourcing.Backbone.Tests
{
    /// <summary>
    /// The end to end explicit tests.
    /// </summary>
    public class EndToEndExplicitTests : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IEventFlowConsumer _subscriber = A.Fake<IEventFlowConsumer>();
        private readonly IProducerStoreStrategyBuilder _producerBuilder;
        private readonly IConsumerStoreStrategyBuilder _consumerBuilder;

        private readonly string URI = $"{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}:{Guid.NewGuid():N}:some-shard-{DateTime.UtcNow.Second}";

        private readonly ILogger _fakeLogger = A.Fake<ILogger>();

        private static readonly Person _person = new Person(10, "Tom");
        private static readonly JsonElement _personElement = JsonDocument.Parse(File.ReadAllText("person.json")).RootElement;
        private static readonly JsonElement _payloadElement = JsonDocument.Parse(File.ReadAllText("payload.json")).RootElement;
        private readonly string ENV = $"test";
        private const int TIMEOUT = 1000 * 20;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="EndToEndExplicitTests"/> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
        /// <param name="producerChannelBuilder">The producer channel builder.</param>
        /// <param name="consumerChannelBuilder">The consumer channel builder.</param>
        public EndToEndExplicitTests(
            ITestOutputHelper outputHelper,
            Func<IProducerStoreStrategyBuilder, ILogger, IProducerStoreStrategyBuilder>? producerChannelBuilder = null,
             Func<IConsumerStoreStrategyBuilder, ILogger, IConsumerStoreStrategyBuilder>? consumerChannelBuilder = null)
        {
            _outputHelper = outputHelper;
            _producerBuilder = ProducerBuilder.Empty.UseRedisChannel();
            _producerBuilder = producerChannelBuilder?.Invoke(_producerBuilder, _fakeLogger) ?? _producerBuilder;

            _consumerBuilder = ConsumerBuilder.Empty.UseRedisChannel();
            _consumerBuilder = consumerChannelBuilder?.Invoke(_consumerBuilder, _fakeLogger) ?? _consumerBuilder;

            A.CallTo(() => _subscriber.Stage1Async(A<Person>.Ignored, A<string>.Ignored))
                    .ReturnsLazily(() => ValueTask.CompletedTask);
            A.CallTo(() => _subscriber.Stage2Async(A<JsonElement>.Ignored, A<JsonElement>.Ignored))
                    .ReturnsLazily(() => Delay());

            #region  A.CallTo(() => _fakeLogger...)

            A.CallTo(() => _fakeLogger.Log<string>(
                A<LogLevel>.Ignored,
                A<EventId>.Ignored,
                A<string>.Ignored,
                A<Exception>.Ignored,
                A<Func<string, Exception, string>>.Ignored
                ))
                .Invokes<object, LogLevel, EventId, string, Exception, Func<string, Exception, string>>((level, id, msg, ex, fn) =>
                       _outputHelper.WriteLine(
                        $"Info: {fn(msg, ex)}"));

            #endregion //  A.CallTo(() => _fakeLogger...)

            async ValueTask Delay() => await Task.Delay(200);

        }

        #endregion // Ctor

        #region OnSucceed_ACK_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task OnSucceed_ACK_Test()
        {
            #region IEventFlow producer = ...

            IEventFlowProducer producer = _producerBuilder
                                            .Environment(ENV)
                                            //.WithOptions(producerOption)
                                            .Uri(URI)
                                            //.WithLogger(_fakeLogger)
                                            .BuildEventFlowProducer();

            #endregion // IEventFlow producer = ...

            await SendSequenceAsync(producer);

            var consumerOptions = new ConsumerOptions
            {
                AckBehavior = AckBehavior.OnSucceed,
                MaxMessages = 2 /* detach consumer after 2 messages*/
            };
            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            IConsumerSubscribeBuilder builder = _consumerBuilder
                         .WithOptions(o => consumerOptions)
                             .WithCancellation(cancellation)
                             .Environment(ENV)
                             .Uri(URI)
                             .WithLogger(_fakeLogger);
            await using IConsumerLifetime subscription = builder
                                        .Group("CONSUMER_GROUP_1")
                                        .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                                        .SubscribeEventFlowConsumer(_subscriber);

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await subscription.Completion;

            #region Validation

            A.CallTo(() => _subscriber.Stage1Async(A<Person>.Ignored, A<string>.Ignored))
                        .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.Stage2Async(A<JsonElement>.Ignored, A<JsonElement>.Ignored))
                        .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // OnSucceed_ACK_Test

        #region SendSequenceAsync

        /// <summary>
        /// Sends standard test sequence.
        /// </summary>
        /// <param name="producer">The producer.</param>
        private static async Task SendSequenceAsync(IEventFlowProducer producer)
        {
            await producer.Stage1Async(_person, "STAGE 1");
            await producer.Stage2Async(_personElement, _payloadElement);
        }

        #endregion // SendSequenceAsync

        #region GetCancellationToken

        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        /// <returns></returns>
        private static CancellationToken GetCancellationToken()
        {
            return new CancellationTokenSource(Debugger.IsAttached
                                ? TimeSpan.FromMinutes(10)
                                : TimeSpan.FromSeconds(10)).Token;
        }

        #endregion // GetCancellationToken

        #region Dispose pattern

        ~EndToEndExplicitTests()
        {
            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            string key = URI;
            IConnectionMultiplexer conn = RedisClientFactory.CreateProviderAsync(
                                                    logger: _fakeLogger,
                                                    configurationHook: cfg => cfg.AllowAdmin = true).Result;
            IDatabaseAsync db = conn.GetDatabase();

            db.KeyDeleteAsync(key, CommandFlags.DemandMaster).Wait();
        }

        #endregion // Dispose pattern
    }
}
