using FakeItEasy;

using Microsoft.Extensions.Logging;

using Polly;

using StackExchange.Redis;

using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.Channels.RedisProvider;
using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;


// TODO: [bnaya 2020-10] ensure message order(cancel ack should cancel all following messages)
// TODO: [bnaya 2020-10] check for no pending

namespace Weknow.EventSource.Backbone.Tests
{
    public class EndToEndExplicitTests : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IEventFlow _subscriber = A.Fake<IEventFlow>();
        private readonly IProducerStoreStrategyBuilder _producerBuilder;
        private readonly IConsumerStoreStrategyBuilder _consumerBuilder;

        private string PARTITION = $"test-{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}-{Guid.NewGuid():N}";
        private string SHARD = $"some-shard-{DateTime.UtcNow.Second}";

        private ILogger _fakeLogger = A.Fake<ILogger>();

        private static readonly Person _person = new Person(10, "Tom");
        private static readonly JsonElement _personElement = JsonDocument.Parse(File.ReadAllText("person.json")).RootElement;
        private static readonly JsonElement _payloadElement = JsonDocument.Parse(File.ReadAllText("payload.json")).RootElement;

        #region Ctor

        public EndToEndExplicitTests(
            ITestOutputHelper outputHelper,
            Func<IProducerStoreStrategyBuilder, ILogger, IProducerStoreStrategyBuilder> producerChannelBuilder = null,
             Func<IConsumerStoreStrategyBuilder, ILogger, IConsumerStoreStrategyBuilder> consumerChannelBuilder = null)
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

        [Fact]
        public async Task OnSucceed_ACK_Test()
        {
            #region IEventFlow producer = ...

            IEventFlowProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Partition(PARTITION)
                                            .Shard(SHARD)
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
                             .Partition(PARTITION)
                             .Shard(SHARD)
                             .WithLogger(_fakeLogger);
            await using IConsumerLifetime subscription = builder.SubscribeEventFlow(_subscriber, "CONSUMER_GROUP_1", $"TEST {DateTime.UtcNow:HH:mm:ss}");
            //.SubscribeEventFlow(_subscriber, "CONSUMER_GROUP_1", $"TEST {DateTime.UtcNow:HH:mm:ss}");

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
            string END_POINT_KEY = "REDIS_EVENT_SOURCE_PRODUCER_ENDPOINT";
            string PASSWORD_KEY = "REDIS_EVENT_SOURCE_PRODUCER_PASS";
            string key = $"{PARTITION}:{SHARD}";
            IDatabaseAsync db = RedisClientFactory.CreateAsync(
                                                _fakeLogger,
                                                cfg => cfg.AllowAdmin = true,
                                                endpointKey: END_POINT_KEY,
                                                passwordKey: PASSWORD_KEY).Result;

            db.KeyDeleteAsync(key, CommandFlags.DemandMaster).Wait();
        }

        #endregion // Dispose pattern

        private class Subscriber : IEventFlow
        {
            private readonly IEventFlow _subscriber;

            public Subscriber(IEventFlow subscriber)
            {
                _subscriber = subscriber;
            }

            ValueTask IEventFlow.Stage1Async(Person PII, string payload) => _subscriber.Stage1Async(PII, payload);

            ValueTask IEventFlow.Stage2Async(JsonElement PII, JsonElement data) => _subscriber.Stage2Async(PII, data);
        }

    }
}
