#pragma warning disable S3881 // "IDisposable" should be implemented correctly

using System.Collections.Concurrent;
using System.Diagnostics;

using EventSourcing.Backbone.Building;
using EventSourcing.Backbone.Channels.RedisProvider;
using EventSourcing.Backbone.Enums;
using EventSourcing.Backbone.Tests.Entities;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

// docker run -p 6379:6379 -it --rm --name redis-evt-src redislabs/rejson:latest

namespace EventSourcing.Backbone.Tests
{
    /// <summary>
    /// The end to end tests.
    /// </summary>
    public class InheritanceTests : TestsBase
    {
        private readonly IFlowAConsumer _subscriberA = A.Fake<IFlowAConsumer>();
        private readonly IFlowBConsumer _subscriberB = A.Fake<IFlowBConsumer>();
        private readonly IFlowABConsumer _subscriberAB = A.Fake<IFlowABConsumer>();

        private readonly IProducerStoreStrategyBuilder _producerBuilder;
        private readonly IConsumerStoreStrategyBuilder _consumerBuilder;

        private readonly string ENV = $"test";
        protected override string URI { get; } = $"{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}:{Guid.NewGuid():N}";


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
            : base(outputHelper)
        {
            _producerBuilder = ProducerBuilder.Empty.UseRedisChannel( /*,
                                        configuration: (cfg) => cfg.ServiceName = "mymaster" */);
            _producerBuilder = producerChannelBuilder?.Invoke(_producerBuilder, _fakeLogger) ?? _producerBuilder;
            RedisConsumerChannelSetting stg = new RedisConsumerChannelSetting
            {
                DelayWhenEmptyBehavior = new DelayWhenEmptyBehavior
                {
                    CalcNextDelay = ((d, _) => TimeSpan.FromMilliseconds(2))
                }
            };
            var consumerBuilder = stg.CreateRedisConsumerBuilder();
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

        [Fact(Timeout = TIMEOUT)]
        public async Task Inheritance_PartialConsumer_Strict_Succeed_Test()
        {
            #region ISequenceOperations producer = ...

            IFlowABProducer producer = _producerBuilder
                                            .Environment(ENV)
                                            .Uri(URI)
                                            .BuildFlowABProducer();

            #endregion // ISequenceOperations producer = ...

            await producer.AAsync(1);
            var now = DateTimeOffset.Now;
            await producer.BAsync(now);
            await producer.DerivedAsync("Hi");

            CancellationToken cancellation = GetCancellationToken();

            #region Prepare

            var hash = new ConcurrentDictionary<string, int>();
            A.CallTo(() => _subscriberA.AAsync(A<ConsumerMetadata>.Ignored, 1))
                .Invokes(c => { hash.AddOrUpdate(c.Method.Name, 1, (k, v) => v + 1); });
            A.CallTo(() => _subscriberB.BAsync(A<ConsumerMetadata>.Ignored, A<DateTimeOffset>.Ignored))
            .Invokes(c => { hash.AddOrUpdate(c.Method.Name, 1, (k, v) => v + 1); });
            A.CallTo(() => _subscriberAB.DerivedAsync(A<ConsumerMetadata>.Ignored, "Hi"))
                .Invokes(c => { hash.AddOrUpdate(c.Method.Name, 1, (k, v) => v + 1); });
            A.CallTo(() => _subscriberAB.AAsync(A<ConsumerMetadata>.Ignored, 1))
                .Invokes(c => { hash.AddOrUpdate(c.Method.Name, 1, (k, v) => v + 1); });
            A.CallTo(() => _subscriberAB.BAsync(A<ConsumerMetadata>.Ignored, A<DateTimeOffset>.Ignored))
                .Invokes(c => { hash.AddOrUpdate(c.Method.Name, 1, (k, v) => v + 1); });

            #endregion // Prepare

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            var consumerBuilder = _consumerBuilder
                             .WithOptions(o => DefaultOptions(o, 3, AckBehavior.OnSucceed))
                             .WithCancellation(cancellation)
                             .Environment(ENV)
                             .Uri(URI)
                             .Name($"TEST {DateTime.UtcNow:HH:mm:ss}");
            await using IConsumerLifetime subscription1 = consumerBuilder
                            .Group("CONSUMER_GROUP_X_1")
                            .SubscribeFlowAConsumer(_subscriberA);
            await using IConsumerLifetime subscription2 = consumerBuilder
                            .Group("CONSUMER_GROUP_X_2")
                            .SubscribeFlowBConsumer(_subscriberB);
            await using IConsumerLifetime subscription3 = consumerBuilder
                            .Group("CONSUMER_GROUP_X_3")
                            .SubscribeFlowABConsumer(_subscriberAB);

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await subscription1.Completion;
            await subscription2.Completion;
            await subscription3.Completion;

            #region Validation


            Assert.Equal(3, hash.Count);
            Assert.True(hash.All(m => m.Value >= 1));
            A.CallTo(() => _subscriberA.AAsync(A<ConsumerMetadata>.Ignored, 1))
               .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberB.BAsync(A<ConsumerMetadata>.Ignored, A<DateTimeOffset>.Ignored))
               .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberAB.DerivedAsync(A<ConsumerMetadata>.Ignored, "Hi"))
               .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberAB.AAsync(A<ConsumerMetadata>.Ignored, 1))
               .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberAB.BAsync(A<ConsumerMetadata>.Ignored, A<DateTimeOffset>.Ignored))
               .MustHaveHappenedOnceExactly();

            #endregion // Validation

        }

        #endregion // Inheritance_PartialConsumer_Strict_Succeed_Test

        #region Inheritance_ConsumerCooperation_Succeed_Test

        // TODO: [bnaya 2023-02-16] check why `MaxMessages` don't take effect (ended by cancellation)
        [Fact(Timeout = TIMEOUT)]
        public async Task Inheritance_ConsumerCooperation_Succeed_Test()
        {
            #region ISequenceOperations producer = ...

            IFlowABProducer producer = _producerBuilder
                                            .Environment(ENV)
                                            .Uri(URI)
                                            .BuildFlowABProducer();

            #endregion // ISequenceOperations producer = ...

            await producer.AAsync(1);
            var now = DateTimeOffset.Now;
            await producer.BAsync(now);
            await producer.DerivedAsync("Hi");

            CancellationToken cancellation = GetCancellationToken(10);
            int i = 0;
            var tcs = new TaskCompletionSource<int>();
            #region Prepare

            var hash = new ConcurrentDictionary<string, int>();
            A.CallTo(() => _subscriberA.AAsync(A<ConsumerMetadata>.Ignored, 1))
                .Invokes(c =>
                {
                    hash.AddOrUpdate(c.Method.Name, 1, (k, v) => v + 1);
                    if (Interlocked.Increment(ref i) == 3)
                        tcs.SetResult(i);
                });
            A.CallTo(() => _subscriberB.BAsync(A<ConsumerMetadata>.Ignored, A<DateTimeOffset>.Ignored))
                .Invokes(c =>
                {
                    hash.AddOrUpdate(c.Method.Name, 1, (k, v) => v + 1);
                    if (Interlocked.Increment(ref i) == 3)
                        tcs.SetResult(i);
                });
            A.CallTo(() => _subscriberAB.DerivedAsync(A<ConsumerMetadata>.Ignored, "Hi"))
                .Invokes(c =>
                {
                    hash.AddOrUpdate(c.Method.Name, 1, (k, v) => v + 1);
                    if (Interlocked.Increment(ref i) == 3)
                        tcs.SetResult(i);
                });
            A.CallTo(() => _subscriberAB.AAsync(A<ConsumerMetadata>.Ignored, 1))
                .Invokes(c =>
                {
                    hash.AddOrUpdate(c.Method.Name, 1, (k, v) => v + 1);
                    if (Interlocked.Increment(ref i) == 3)
                        tcs.SetResult(i);
                });
            A.CallTo(() => _subscriberAB.BAsync(A<ConsumerMetadata>.Ignored, A<DateTimeOffset>.Ignored))
                .Invokes(c =>
                {
                    hash.AddOrUpdate(c.Method.Name, 1, (k, v) => v + 1);
                    if (Interlocked.Increment(ref i) == 3)
                        tcs.SetResult(i);
                });

            #endregion // Prepare

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            var consumerBuilder = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, 3, AckBehavior.OnSucceed) with
                         {
                             PartialBehavior = PartialConsumerBehavior.Sequential
                         })
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .Group("CONSUMER_GROUP_X_1");

            await using IConsumerLifetime subscriptionA =
                                consumerBuilder
                                     .WithOptions(c => c with { MaxMessages = 1 })
                                     .Name($"TEST A {DateTime.UtcNow:HH:mm:ss}")
                                     .SubscribeFlowAConsumer(_subscriberA);

            await using IConsumerLifetime subscriptionB =
                                consumerBuilder
                                     .WithOptions(c => c with { MaxMessages = 1 })
                                     .Name($"TEST B {DateTime.UtcNow:HH:mm:ss}")
                                     .SubscribeFlowBConsumer(_subscriberB);

            await using IConsumerLifetime subscriptionAB =
                                consumerBuilder
                                     .Name($"TEST AB {DateTime.UtcNow:HH:mm:ss}")
                                     .SubscribeFlowABConsumer(_subscriberAB);

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await tcs.Task.WithCancellation(new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token);

            #region Validation


            Assert.Equal(3, hash.Count);
            Assert.True(hash.All(m => m.Value == 1));

            #endregion // Validation
        }

        #endregion // Inheritance_ConsumerCooperation_Succeed_Test
    }
}
