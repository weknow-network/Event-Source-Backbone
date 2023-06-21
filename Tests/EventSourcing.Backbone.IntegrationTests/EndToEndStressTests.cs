using System.Diagnostics;

using EventSourcing.Backbone.Building;
using EventSourcing.Backbone.Channels.RedisProvider;
using EventSourcing.Backbone.UnitTests.Entities;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

#pragma warning disable S3881 // "IDisposable" should be implemented correctly

// docker run -p 6379:6379 -it --rm --name redis-event-source redislabs/rejson:latest

namespace EventSourcing.Backbone.Tests
{
    public class EndToEndStressTests : TestsBase
    {
        private readonly ISequenceOperationsConsumer _subscriber = A.Fake<ISequenceOperationsConsumer>();
        private readonly IProducerStoreStrategyBuilder _producerBuilder;
        private readonly IConsumerHooksBuilder _consumerBuilder;

        private readonly string ENV = $"test";
        protected override string URI { get; } = $"{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}:{Guid.NewGuid():N}";

        #region Ctor

        public EndToEndStressTests(
            ITestOutputHelper outputHelper,
            Func<IProducerStoreStrategyBuilder, ILogger, IProducerStoreStrategyBuilder>? producerChannelBuilder = null,
             Func<IConsumerStoreStrategyBuilder, ILogger, IConsumerStoreStrategyBuilder>? consumerChannelBuilder = null)
            : base(outputHelper)
        {
            _producerBuilder = ProducerBuilder.Empty.UseRedisChannel( /*,
                                        configuration: (cfg) => cfg.ServiceName = "mymaster" */);
            _producerBuilder = producerChannelBuilder?.Invoke(_producerBuilder, _fakeLogger) ?? _producerBuilder;

            var stg = new RedisConsumerChannelSetting
            {
                DelayWhenEmptyBehavior = new DelayWhenEmptyBehavior
                {
                    CalcNextDelay = ((d, _) => TimeSpan.FromMilliseconds(2))
                }
            };
            var consumerBuilder = stg.CreateRedisConsumerBuilder();
            consumerBuilder = consumerChannelBuilder?.Invoke(consumerBuilder, _fakeLogger) ?? consumerBuilder;
            var claimTrigger = new ClaimingTrigger { EmptyBatchCount = 5, MinIdleTime = TimeSpan.FromSeconds(3) };
            _consumerBuilder = consumerBuilder.WithOptions(o => o with { ClaimingTrigger = claimTrigger });

            A.CallTo(() => _subscriber.RegisterAsync(A<ConsumerMetadata>.Ignored, A<User>.Ignored))
                    .ReturnsLazily(() => ValueTask.CompletedTask);
            A.CallTo(() => _subscriber.LoginAsync(A<ConsumerMetadata>.Ignored, A<string>.Ignored, A<string>.Ignored))
                    .ReturnsLazily(() => Delay());
            A.CallTo(() => _subscriber.EarseAsync(A<ConsumerMetadata>.Ignored, A<int>.Ignored))
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

        #region Receiver_Stress_Test

        [Fact(Timeout = TIMEOUT)]
        [Trait("type", "stress")]
        public async Task Receiver_Stress_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment(ENV)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            EventKey key = await SendLongSequenceAsync(producer);

            CancellationToken cancellation = GetCancellationToken();

            #region IConsumerReceiver receiver = ...

            IConsumerReceiver receiver = _consumerBuilder
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .BuildReceiver();

            #endregion // IConsumerReceiver receiver = ...

            var res0 = await receiver.GetByIdAsync(key);
            Assert.True(res0.Data.TryGet("id", out int id));
            Assert.Equal(1500, id);
        }

        #endregion // Receiver_Stress_Test

        #region Receiver_Json_Stress_Test

        [Fact(Timeout = TIMEOUT)]
        [Trait("type", "stress")]
        public async Task Receiver_Json_Stress_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment(ENV)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            CancellationToken cancellation = GetCancellationToken();

            #region IConsumerReceiver receiver - ...

            IConsumerReceiver receiver = _consumerBuilder
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .BuildReceiver();

            #endregion IConsumerReceiver receiver...

            await ValidateLongSequenceAsync(producer, receiver);
        }

        #endregion // Receiver_Json_Stress_Test

        #region ValidateLongSequenceAsync

        private async Task ValidateLongSequenceAsync(
            ISequenceOperationsProducer producer, IConsumerReceiver receiver)
        {
            var tasks = Enumerable.Range(1, 1500)
                .Select(async m =>
                {
                    EventKey key = await producer.SuspendAsync(m);
                    _outputHelper.WriteLine($"{m} -->");
                    var res0 = await receiver.GetJsonByIdAsync(key);
                    _outputHelper.WriteLine($"--> {m}");
                    Assert.True(res0.TryGetProperty("id", out var id));
                    Assert.Equal(m, id.GetInt32());
                });
            await Task.WhenAll(tasks);
        }

        #endregion // ValidateLongSequenceAsync

        #region SendLongSequenceAsync

        private static async Task<EventKey> SendLongSequenceAsync(
            ISequenceOperationsProducer producer)
        {
            var tasks = Enumerable.Range(1, 1500)
                .Select(async m => await producer.SuspendAsync(m));
            EventKeys[] ids = await Task.WhenAll(tasks);
            return ids[ids.Length - 1].First();
        }

        #endregion // SendLongSequenceAsync

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
    }
}
