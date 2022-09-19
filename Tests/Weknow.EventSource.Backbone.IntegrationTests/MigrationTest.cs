using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;

using static Weknow.EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;
#pragma warning disable ConstFieldDocumentationHeader // The field must have a documentation header.

// docker run -p 6379:6379 -it --rm --name redis-event-source redislabs/rejson:latest

namespace Weknow.EventSource.Backbone.Tests
{
    /// <summary>
    /// The end to end tests.
    /// </summary>
    public class MigrationTest : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly ISequenceOperationsConsumer _subscriber = A.Fake<ISequenceOperationsConsumer>();
        private readonly SequenceOperationsConsumerBridge _subscriberBridge;
        private readonly IProducerStoreStrategyBuilder _producerBuilder;
        private readonly IConsumerStoreStrategyBuilder _consumerBuilder;
        private readonly string ENV = $"Development";
        private readonly string PARTITION = $"{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}:{Guid.NewGuid():N}";
        private readonly string SHARD = $"some-shard-{DateTime.UtcNow.Second}";

        private readonly ILogger _fakeLogger = A.Fake<ILogger>();
        private static readonly User USER = new User { Eracure = new Personal { Name = "mike", GovernmentId = "A25" }, Comment = "Do it" };
        private const int TIMEOUT = 1_000 * 30;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationTest"/> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
        /// <param name="producerChannelBuilder">The producer channel builder.</param>
        /// <param name="consumerChannelBuilder">The consumer channel builder.</param>
        public MigrationTest(
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
            _consumerBuilder = consumerChannelBuilder?.Invoke(consumerBuilder, _fakeLogger) ?? consumerBuilder;

            A.CallTo(() => _subscriber.RegisterAsync(A<User>.Ignored))
                    .ReturnsLazily(() =>
                    {
                        Metadata meta = ConsumerMetadata.Context;
                        if (string.IsNullOrEmpty(meta.EventKey))
                            return ValueTask.FromException(new Exception("Event Key is missing"));
                        return ValueTask.CompletedTask;
                    });
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

            _subscriberBridge = new SequenceOperationsConsumerBridge(_subscriber);
        }

        #endregion // Ctor


        private ConsumerOptions DefaultOptions(
                    ConsumerOptions options,
                    uint? maxMessages = null,
                    AckBehavior? ackBehavior = null,
                    PartialConsumerBehavior? behavior = null)
        {
            var claimTrigger = new ClaimingTrigger { EmptyBatchCount = 5, MinIdleTime = TimeSpan.FromSeconds(3) };
            return options with
            {
                ClaimingTrigger = claimTrigger,
                MaxMessages = maxMessages ?? options.MaxMessages,
                AckBehavior = ackBehavior ?? options.AckBehavior,
                PartialBehavior = behavior ?? options.PartialBehavior
            };
        }

        #region Migration_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task Migration_Test()
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

            IRawProducer rawProducer = _producerBuilder
                                            .Environment("Migrated")                                            
                                            .BuildRaw();

            await SendSequenceAsync(producer);
            SubscriptionBridge subscriberBridge = new (rawProducer);

            CancellationToken cancellation = GetCancellationToken();

            var consumerBuilder = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, 3, AckBehavior.OnSucceed))
                         .WithCancellation(cancellation)
                         .Partition(PARTITION)
                         .Shard(SHARD)
                         .WithLogger(_fakeLogger);

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = consumerBuilder
                         .Environment(ENV)
                         .Group("CONSUMER_GROUP_1")
                         .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                         .Subscribe(subscriberBridge);

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            #region  await using IConsumerLifetime subscriptionMigration = ...Subscribe(...)

            await using IConsumerLifetime subscriptionMigration = consumerBuilder
                         .Environment("Migrated")
                         .Group("CONSUMER_GROUP_2")
                         .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                         .Subscribe(_subscriberBridge);

            #endregion //  await using IConsumerLifetime subscriptionMigration = ...Subscribe(...)

            await subscription.Completion;
            await subscriptionMigration.Completion;

            #region Validation

            A.CallTo(() => _subscriber.RegisterAsync(A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.LoginAsync("admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.EarseAsync(4335))
                .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // Migration_Test

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


        ~MigrationTest()
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
                foreach (string key in keys)
                {
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


        private class SubscriptionBridge : ISubscriptionBridge
        {
            private readonly IRawProducer _fw;

            public SubscriptionBridge(IRawProducer fw)
            {
                _fw = fw;
            }

            public async Task<bool> BridgeAsync(Announcement announcement, IConsumerBridge consumerBridge)
            {
                await _fw.Produce(announcement);
                return true;

            }
        }

    }
}
