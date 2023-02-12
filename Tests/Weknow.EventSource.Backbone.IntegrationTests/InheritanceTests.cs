using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Polly;

using StackExchange.Redis;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.Enums;
using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;

using static Weknow.EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;
using static Weknow.EventSource.Backbone.EventSourceConstants;

// docker run -p 6379:6379 -it --rm --name redis-event-source redislabs/rejson:latest

namespace Weknow.EventSource.Backbone.Tests
{
    /// <summary>
    /// The end to end tests.
    /// </summary>
    public class InheritanceTests : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IFlowAConsumer _subscriberA = A.Fake<IFlowAConsumer>();
        private readonly IFlowBConsumer _subscriberB = A.Fake<IFlowBConsumer>();
        private readonly IFlowABConsumer _subscriberAB = A.Fake<IFlowABConsumer>();
        //private readonly FlowAConsumerBridge _subscriberBridgeA;
        //private readonly FlowBConsumerBridge _subscriberBridgeB;
        //private readonly FlowABConsumerBridge _subscriberBridgeAB;

        private readonly IProducerStoreStrategyBuilder _producerBuilder;
        private readonly IConsumerStoreStrategyBuilder _consumerBuilder;

        private readonly string ENV = $"Development";
        private readonly string PARTITION = $"{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}:{Guid.NewGuid():N}";
        private readonly string SHARD = $"some-shard-{DateTime.UtcNow.Second}";

        private readonly ILogger _fakeLogger = A.Fake<ILogger>();
        private const int TIMEOUT = 1_000 * 50;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InheritanceTests"/> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
        /// <param name="producerChannelBuilder">The producer channel builder.</param>
        /// <param name="consumerChannelBuilder">The consumer channel builder.</param>
        public InheritanceTests(
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
        }

        #endregion // Ctor

        #region DefaultOptions

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

        #endregion // DefaultOptions

        #region Inheritance_PartialConsumer_Strict_Succeed_Test

        [Theory(Timeout = TIMEOUT)]
        [InlineData(MultiConsumerBehavior.All)]
        [InlineData(MultiConsumerBehavior.Once)]
        public async Task Inheritance_PartialConsumer_Strict_Succeed_Test(MultiConsumerBehavior multiConsumerBehavior)
        {
            #region ISequenceOperations producer = ...

            IFlowABProducer producer = _producerBuilder
                                            .Environment(ENV)
                                            .Partition(PARTITION)
                                            .Shard(SHARD)
                                            .BuildFlowABProducer();

            #endregion // ISequenceOperations producer = ...

            var p = new Person(100, "bnaya");
            await producer.AAsync(1);
            var now = DateTimeOffset.Now;
            await producer.BAsync(now);
            await producer.DerivedAsync("Hi");

            CancellationToken cancellation = GetCancellationToken();

            #region Prepare

            var hash = new ConcurrentDictionary<string, int>();
            A.CallTo(() => _subscriberA.AAsync(1))
                .Invokes(c => { hash.AddOrUpdate(c.Method.Name, 1, (k, v) => v + 1); });
            A.CallTo(() => _subscriberB.BAsync(A<DateTimeOffset>.Ignored))
            .Invokes(c => { hash.AddOrUpdate(c.Method.Name, 1, (k, v) => v + 1); });
            A.CallTo(() => _subscriberAB.DerivedAsync("Hi"))
                .Invokes(c => { hash.AddOrUpdate(c.Method.Name, 1, (k, v) => v + 1); });
            A.CallTo(() => _subscriberAB.AAsync(1))
                .Invokes(c => { hash.AddOrUpdate(c.Method.Name, 1, (k, v) => v + 1); });
            A.CallTo(() => _subscriberAB.BAsync(A<DateTimeOffset>.Ignored))
                .Invokes(c => { hash.AddOrUpdate(c.Method.Name, 1, (k, v) => v + 1); });

            #endregion // Prepare

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, 3, AckBehavior.OnSucceed) with
                         {
                             MultiConsumerBehavior = multiConsumerBehavior
                         })
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Partition(PARTITION)
                         .Shard(SHARD)
                         .Group("CONSUMER_GROUP_X_1")
                         .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                         .SubscribeFlowAConsumer(_subscriberA)
                         .SubscribeFlowBConsumer(_subscriberB)
                         .SubscribeFlowABConsumer(_subscriberAB);

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await subscription.Completion;

            #region Validation


            Assert.Equal(3, hash.Count);
            if (multiConsumerBehavior == MultiConsumerBehavior.All)
            {
                Assert.True(hash.All(m => m.Value >= 1));
                A.CallTo(() => _subscriberA.AAsync(1))
                   .MustHaveHappenedOnceExactly();
                A.CallTo(() => _subscriberB.BAsync(A<DateTimeOffset>.Ignored))
                   .MustHaveHappenedOnceExactly();
                A.CallTo(() => _subscriberAB.DerivedAsync("Hi"))
                   .MustHaveHappenedOnceExactly();
                A.CallTo(() => _subscriberAB.AAsync(1))
                   .MustHaveHappenedOnceExactly();
                A.CallTo(() => _subscriberAB.BAsync(A<DateTimeOffset>.Ignored))
                   .MustHaveHappenedOnceExactly();
            }
            else
                Assert.True(hash.All(m => m.Value == 1));

            #endregion // Validation

        }

        #endregion // Inheritance_PartialConsumer_Strict_Succeed_Test

        #region Inheritance_ConsumerCooperation_Succeed_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task Inheritance_ConsumerCooperation_Succeed_Test()
        {
            #region ISequenceOperations producer = ...

            IFlowABProducer producer = _producerBuilder
                                            .Environment(ENV)
                                            .Partition(PARTITION)
                                            .Shard(SHARD)
                                            .BuildFlowABProducer();

            #endregion // ISequenceOperations producer = ...

            var p = new Person(100, "bnaya");
            await producer.AAsync(1);
            var now = DateTimeOffset.Now;
            await producer.BAsync(now);
            await producer.DerivedAsync("Hi");

            CancellationToken cancellation = GetCancellationToken();

            #region Prepare

            var hash = new ConcurrentDictionary<string, int>();
            A.CallTo(() => _subscriberA.AAsync(1))
                .Invokes(c => { hash.AddOrUpdate(c.Method.Name, 1, (k, v) => v + 1); });
            A.CallTo(() => _subscriberB.BAsync(A<DateTimeOffset>.Ignored))
            .Invokes(c => { hash.AddOrUpdate(c.Method.Name, 1, (k, v) => v + 1); });
            A.CallTo(() => _subscriberAB.DerivedAsync("Hi"))
                .Invokes(c => { hash.AddOrUpdate(c.Method.Name, 1, (k, v) => v + 1); });
            A.CallTo(() => _subscriberAB.AAsync(1))
                .Invokes(c => { hash.AddOrUpdate(c.Method.Name, 1, (k, v) => v + 1); });
            A.CallTo(() => _subscriberAB.BAsync(A<DateTimeOffset>.Ignored))
                .Invokes(c => { hash.AddOrUpdate(c.Method.Name, 1, (k, v) => v + 1); });

            #endregion // Prepare

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            var consumerBuilder = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, 3, AckBehavior.OnSucceed) with 
                         {
                             PartialBehavior = PartialConsumerBehavior.Sequential 
                         })
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Partition(PARTITION)
                         .Shard(SHARD)
                         .Group("CONSUMER_GROUP_X_1");

            await using IConsumerLifetime subscriptionA =
                                consumerBuilder
                                     .Name($"TEST A {DateTime.UtcNow:HH:mm:ss}")
                                     .SubscribeFlowAConsumer(_subscriberA);

            await using IConsumerLifetime subscriptionB =
                                consumerBuilder
                                     .Name($"TEST B {DateTime.UtcNow:HH:mm:ss}")
                                     .SubscribeFlowBConsumer(_subscriberB);

            await using IConsumerLifetime subscriptionAB =
                                consumerBuilder
                                     .Name($"TEST AB {DateTime.UtcNow:HH:mm:ss}")
                                     .SubscribeFlowABConsumer(_subscriberAB);

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await subscriptionA.Completion;
            await subscriptionB.Completion;
            await subscriptionAB.Completion;

            #region Validation


            Assert.Equal(3, hash.Count);
            Assert.True(hash.All(m => m.Value == 1));

            #endregion // Validation

        }

        #endregion // Inheritance_ConsumerCooperation_Succeed_Test

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


        ~InheritanceTests()
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
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
                foreach (string key in keys)
                {
                    ab.Post(key);
                }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

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