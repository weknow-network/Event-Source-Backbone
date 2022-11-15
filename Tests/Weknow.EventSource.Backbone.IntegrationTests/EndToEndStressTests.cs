using FakeItEasy;

using Microsoft.Extensions.Logging;

using Polly;

using StackExchange.Redis;

using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.Channels.RedisProvider;
using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;

using static Weknow.EventSource.Backbone.EventSourceConstants;
using static Weknow.EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;
using System.Threading.Tasks.Dataflow;

// docker run -p 6379:6379 -it --rm --name redis-event-source redislabs/rejson:latest

namespace Weknow.EventSource.Backbone.Tests
{
    public class EndToEndStressTests : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly ISequenceOperationsConsumer _subscriber = A.Fake<ISequenceOperationsConsumer>();
        private readonly IProducerStoreStrategyBuilder _producerBuilder;
        private readonly IConsumerHooksBuilder _consumerBuilder;

        private string ENV = $"test";
        private string PARTITION = $"{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}:{Guid.NewGuid():N}";
        private string SHARD = $"some-shard-{DateTime.UtcNow.Second}";

        private ILogger _fakeLogger = A.Fake<ILogger>();
        private static readonly User USER = new User { Eracure = new Personal { Name = "mike", GovernmentId = "A25" }, Comment = "Do it" };
        private const int TIMEOUT = 1000 * 30;

        #region Ctor

        public EndToEndStressTests(
            ITestOutputHelper outputHelper,
            Func<IProducerStoreStrategyBuilder, ILogger, IProducerStoreStrategyBuilder>? producerChannelBuilder = null,
             Func<IConsumerStoreStrategyBuilder, ILogger, IConsumerStoreStrategyBuilder>? consumerChannelBuilder = null)
        {
            _outputHelper = outputHelper;
            _producerBuilder = ProducerBuilder.Empty.UseRedisChannel( /*,
                                        configuration: (cfg) => cfg.ServiceName = "mymaster" */);
            _producerBuilder = producerChannelBuilder?.Invoke(_producerBuilder, _fakeLogger) ?? _producerBuilder;

            var consumerBuilder = ConsumerBuilder.Empty.UseRedisChannel(
                                        stg => stg with
                                        {
                                            DelayWhenEmptyBehavior =
                                                stg.DelayWhenEmptyBehavior with
                                                {
                                                    CalcNextDelay = (d => TimeSpan.FromMilliseconds(2))
                                                }
                                        });
            consumerBuilder = consumerChannelBuilder?.Invoke(consumerBuilder, _fakeLogger) ?? consumerBuilder;
            var claimTrigger = new ClaimingTrigger { EmptyBatchCount = 5, MinIdleTime = TimeSpan.FromSeconds(3) };
            _consumerBuilder = consumerBuilder.WithOptions(o => o with { ClaimingTrigger = claimTrigger });

            A.CallTo(() => _subscriber.RegisterAsync(A<User>.Ignored))
                    .ReturnsLazily(() => ValueTask.CompletedTask);
            A.CallTo(() => _subscriber.LoginAsync(A<string>.Ignored, A<string>.Ignored))
                    .ReturnsLazily(() => Delay());
            A.CallTo(() => _subscriber.EarseAsync(A<int>.Ignored))
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
                                            .Partition(PARTITION)
                                            .Shard(SHARD)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            EventKey key = await SendLongSequenceAsync(producer);

            CancellationToken cancellation = GetCancellationToken();

            #region IConsumerReceiver receiver = ...

            IConsumerReceiver receiver = _consumerBuilder
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Partition(PARTITION)
                         .Shard(SHARD)
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
                                            .Partition(PARTITION)
                                            .Shard(SHARD)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            CancellationToken cancellation = GetCancellationToken();

            #region IConsumerReceiver receiver - ...

            IConsumerReceiver receiver = _consumerBuilder
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Partition(PARTITION)
                         .Shard(SHARD)
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

        #region SendSequenceAsync

        private static async Task SendSequenceAsync(ISequenceOperations producer, string pass = "1234")
        {
            await producer.RegisterAsync(USER);
            await producer.LoginAsync("admin", pass);
            await producer.EarseAsync(4335);
        }

        private static async Task<EventKeys> SendSequenceAsync(ISequenceOperationsProducer producer, string pass = "1234")
        {
            EventKey r1 = await producer.RegisterAsync(USER with { Comment = null });
            EventKey r2 = await producer.LoginAsync("admin", pass);
            EventKey r3 = await producer.EarseAsync(4335);
            return new[] { r1, r2, r3 };
        }
        private static async Task<EventKeys> SendSequenceAsync(IProducerSequenceOperations producer, string pass = "1234")
        {
            EventKey r1 = await producer.RegisterAsync(USER);
            EventKey r2 = await producer.LoginAsync("admin", pass);
            EventKey r3 = await producer.EarseAsync(4335);
            return new[] { r1, r2, r3 };
        }

        #endregion // SendSequenceAsync

        #region SendLongSequenceAsync

        private static async Task<EventKey> SendLongSequenceAsync(ISequenceOperationsProducer producer, string pass = "1234")
        {
            var tasks = Enumerable.Range(1, 1500).Select(async m => await producer.SuspendAsync(m));
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

        #region Dispose pattern


        ~EndToEndStressTests()
        {
            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            try
            {
                IConnectionMultiplexer conn = RedisClientFactory.CreateProviderBlocking(
                                                    cfg => cfg.AllowAdmin = true);
                string serverName = Environment.GetEnvironmentVariable(END_POINT_KEY) ?? "localhost:6379";
                var server = conn.GetServer(serverName);
                IEnumerable<RedisKey> keys = server.Keys(pattern: $"*{PARTITION}*");
                IDatabaseAsync db = conn.GetDatabase();

                var ab = new ActionBlock<string>(k => LocalAsync(k), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 30 });
                foreach (string? key in keys)
                {
                    if (string.IsNullOrEmpty(key)) continue;
                    ab.Post(key);
                }

                ab.Complete();
                ab.Completion.Wait();

                async Task LocalAsync(string k)
                {
                    try
                    {
                        await db.KeyDeleteAsync(k, CommandFlags.DemandMaster);
                        _outputHelper.WriteLine($"Cleanup: delete key [{k}]");
                    }
                    #region Exception Handling

                    catch (RedisTimeoutException ex)
                    {
                        _outputHelper.WriteLine($"Test dispose timeout error (delete keys) {ex.FormatLazy()}");
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine($"Test dispose timeout error (delete keys) {ex.FormatLazy()}");
                    }

                    #endregion // Exception Handling
                }


            }
            #region Exception Handling

            catch (RedisTimeoutException ex)
            {
                _outputHelper.WriteLine($"Test dispose timeout error (delete keys) {ex.FormatLazy()}");
            }
            catch (Exception ex)
            {
                _outputHelper.WriteLine($"Test dispose error (delete keys) {ex.FormatLazy()}");
            }

            #endregion // Exception Handling
        }

        #endregion // Dispose pattern
    }
}
