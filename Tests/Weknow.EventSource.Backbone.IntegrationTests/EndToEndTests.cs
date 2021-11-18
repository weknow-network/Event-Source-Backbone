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

namespace Weknow.EventSource.Backbone.Tests
{
    public class EndToEndTests : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly ISequenceOperations _subscriber = A.Fake<ISequenceOperations>();
        private readonly SequenceOperationsBridge _subscriberBridge;
        private readonly ISequenceOperationsConsumer _autoSubscriber = A.Fake<ISequenceOperationsConsumer>();
        private readonly ISequenceOperations _subscriberPrefix = A.Fake<ISequenceOperations>();
        private readonly ISequenceOperations _subscriberPrefix1 = A.Fake<ISequenceOperations>();
        private readonly ISequenceOperations _subscriberSuffix = A.Fake<ISequenceOperations>();
        private readonly ISequenceOperations _subscriberSuffix1 = A.Fake<ISequenceOperations>();
        private readonly ISequenceOperations _subscriberDynamic = A.Fake<ISequenceOperations>();
        private readonly IProducerStoreStrategyBuilder _producerBuilder;
        private readonly IConsumerStoreStrategyBuilder _consumerBuilder;

        private string PARTITION = $"test-{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}-{Guid.NewGuid():N}";
        private string SHARD = $"some-shard-{DateTime.UtcNow.Second}";

        private ILogger _fakeLogger = A.Fake<ILogger>();
        private static readonly User USER = new User { Eracure = new Personal { Name = "mike", GovernmentId = "A25" }, Comment = "Do it" };

        #region Ctor

        public EndToEndTests(
            ITestOutputHelper outputHelper,
            Func<IProducerStoreStrategyBuilder, ILogger, IProducerStoreStrategyBuilder>? producerChannelBuilder = null,
             Func<IConsumerStoreStrategyBuilder, ILogger, IConsumerStoreStrategyBuilder>? consumerChannelBuilder = null)
        {
            _outputHelper = outputHelper;
            _producerBuilder = ProducerBuilder.Empty.UseRedisChannel( /*,
                                        configuration: (cfg) => cfg.ServiceName = "mymaster" */);
            _producerBuilder = producerChannelBuilder?.Invoke(_producerBuilder, _fakeLogger) ?? _producerBuilder;
            var consumerSetting = RedisConsumerChannelSetting.Default;
            var claimTrigger = consumerSetting.ClaimingTrigger;
            claimTrigger.EmptyBatchCount = 5;
            claimTrigger.MinIdleTime = TimeSpan.FromSeconds(3);
            consumerSetting.DelayWhenEmptyBehavior.CalcNextDelay = d => TimeSpan.FromMilliseconds(2);

            _consumerBuilder = ConsumerBuilder.Empty.UseRedisChannel(
                                        stg => stg with
                                        {
                                            ClaimingTrigger = claimTrigger
                                        });
            _consumerBuilder = consumerChannelBuilder?.Invoke(_consumerBuilder, _fakeLogger) ?? _consumerBuilder;

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

            _subscriberBridge = new SequenceOperationsBridge(_subscriber);
        }

        #endregion // Ctor

        #region Environmet_Test

        [Fact]
        public async Task Environmet_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment("Test")
                                            .Partition(PARTITION)
                                            .Shard(SHARD)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            await SendSequenceAsync(producer);

            var consumerOptions = new ConsumerOptions
            {
                AckBehavior = AckBehavior.OnSucceed,
                MaxMessages = 3 /* detach consumer after 3 messages*/
            };
            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                         .WithOptions(o => consumerOptions)
                         .WithCancellation(cancellation)
                         .Environment("Test")
                         .Partition(PARTITION)
                         .Shard(SHARD)
                         .WithLogger(_fakeLogger)
                         .Subscribe(_subscriberBridge, "CONSUMER_GROUP_1", $"TEST {DateTime.UtcNow:HH:mm:ss}");

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await subscription.Completion;

            #region Validation

            A.CallTo(() => _subscriber.RegisterAsync(A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.LoginAsync("admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.EarseAsync(4335))
                .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // Environmet_Test

        #region Receiver_Test

        [Fact]
        public async Task Receiver_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment("Test")
                                            .Partition(PARTITION)
                                            .Shard(SHARD)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            EventKeys keys = await SendSequenceAsync(producer);

            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            IConsumerReceiver receiver = _consumerBuilder
                         .WithCancellation(cancellation)
                         .Environment("Test")
                         .Partition(PARTITION)
                         .Shard(SHARD)
                         .WithLogger(_fakeLogger)
                         .BuildReceiver();

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            var res0 = await receiver.GetByIdAsync(keys[0]);
            var res1 = await receiver.GetByIdAsync(keys[1]);
            var hasEmail = res1.Data.TryGet("email", out string? email);
            Assert.Equal("admin", email);
            var res2 = await receiver.GetByIdAsync(keys[2]);
            var hasId = res2.Data.TryGet("id", out int id);
            Assert.Equal(4335, id);
        }

        #endregion // Receiver_Test

        #region Receiver_Json_Test

        record TestEmailPass(string email, string password);


        [Fact]
        public async Task Receiver_Json_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment("Test")
                                            .Partition(PARTITION)
                                            .Shard(SHARD)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            EventKeys keys = await SendSequenceAsync(producer);

            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            IConsumerReceiver receiver = _consumerBuilder
                         .WithCancellation(cancellation)
                         .Environment("Test")
                         .Partition(PARTITION)
                         .Shard(SHARD)
                         .WithLogger(_fakeLogger)
                         .BuildReceiver();

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            var res0 = await receiver.GetJsonByIdAsync(keys[0]);
            Assert.True(res0.TryGetProperty("user", out JsonElement uj0));
            var u0 = JsonSerializer.Deserialize<User>(uj0.GetRawText(), SerializerOptionsWithIndent);
            Assert.Equal("A25", u0?.Eracure?.GovernmentId);
            Assert.Equal("mike", u0?.Eracure?.Name);
            var res1 = await receiver.GetJsonByIdAsync(keys[1]);
            var p1 = JsonSerializer.Deserialize<TestEmailPass>(res1.GetRawText(), SerializerOptionsWithIndent);
            Assert.Equal("admin", p1?.email);
            Assert.Equal("1234", p1?.password);
            var res2 = await receiver.GetJsonByIdAsync(keys[2]);
            var pj2 = res2.GetProperty("id");
            Assert.Equal(4335, pj2.GetInt32());
        }

        #endregion // Receiver_Json_Test

        #region OnSucceed_ACK_Test

        [Fact]
        public async Task OnSucceed_ACK_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Partition(PARTITION)
                                            .Shard(SHARD)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            await SendSequenceAsync(producer);

            var consumerOptions = new ConsumerOptions
            {
                AckBehavior = AckBehavior.OnSucceed,
                MaxMessages = 3 /* detach consumer after 3 messages*/
            };
            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                         .WithOptions(o => consumerOptions)
                         .WithCancellation(cancellation)
                         .Partition(PARTITION)
                         .Shard(SHARD)
                         .WithLogger(_fakeLogger)
                         .Subscribe(_subscriberBridge, "CONSUMER_GROUP_1", $"TEST {DateTime.UtcNow:HH:mm:ss}");

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await subscription.Completion;

            #region Validation

            A.CallTo(() => _subscriber.RegisterAsync(A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.LoginAsync("admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.EarseAsync(4335))
                .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // OnSucceed_ACK_Test

        #region GeneratedContract_Test

        [Fact]
        public async Task GeneratedContract_Test()
        {
            #region ISequenceOperations producer1 = ...

            ISequenceOperationsProducer producer1 = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Partition(PARTITION)
                                            .Shard(SHARD)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer1 = ...

            #region ISequenceOperations producer2 = ...

            IProducerSequenceOperations producer2 = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Partition(PARTITION)
                                            .Shard(SHARD)
                                            .WithLogger(_fakeLogger)
                                            .Build(ProducerSequenceOperationsBridgePipeline.Create);

            #endregion // ISequenceOperations producer2 = ...

            await SendSequenceAsync(producer1);
            await SendSequenceAsync(producer2);

            var consumerOptions = new ConsumerOptions
            {
                AckBehavior = AckBehavior.OnSucceed,
                MaxMessages = 6 /* detach consumer after 6 messages*/
            };
            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                         .WithOptions(o => consumerOptions)
                         .WithCancellation(cancellation)
                         .Partition(PARTITION)
                         .Shard(SHARD)
                         .WithLogger(_fakeLogger)
                         .Subscribe(new SequenceOperationsConsumerBridge(_autoSubscriber), "CONSUMER_GROUP_1", $"TEST {DateTime.UtcNow:HH:mm:ss}");

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await subscription.Completion;

            #region Validation

            A.CallTo(() => _autoSubscriber.RegisterAsync(A<User>.Ignored))
                .MustHaveHappenedTwiceExactly();
            A.CallTo(() => _autoSubscriber.LoginAsync("admin", "1234"))
                .MustHaveHappenedTwiceExactly();
            A.CallTo(() => _autoSubscriber.EarseAsync(4335))
                .MustHaveHappenedTwiceExactly();

            #endregion // Validation
        }

        #endregion // GeneratedContract_Test

        #region GeneratedContract_Factory_Test

        [Fact]
        public async Task GeneratedContract_Factory_Test()
        {
            #region ISequenceOperations producer1 = ...

            ISequenceOperationsProducer producer1 = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Partition(PARTITION)
                                            .Shard(SHARD)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer1 = ...

            #region ISequenceOperations producer2 = ...

            IProducerSequenceOperations producer2 = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Partition(PARTITION)
                                            .Shard(SHARD)
                                            .WithLogger(_fakeLogger)
                                            .Build(ProducerSequenceOperationsBridgePipeline.Create);

            #endregion // ISequenceOperations producer2 = ...

            await SendSequenceAsync(producer1);
            await SendSequenceAsync(producer2);

            var consumerOptions = new ConsumerOptions
            {
                AckBehavior = AckBehavior.OnSucceed,
                MaxMessages = 6 /* detach consumer after 6 messages*/
            };
            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                         .WithOptions(o => consumerOptions)
                         .WithCancellation(cancellation)
                         .Partition(PARTITION)
                         .Shard(SHARD)
                         .WithLogger(_fakeLogger)
                         .Subscribe(new SequenceOperationsConsumerBridge(_autoSubscriber), "CONSUMER_GROUP_1", $"TEST {DateTime.UtcNow:HH:mm:ss}");

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await subscription.Completion;

            #region Validation

            A.CallTo(() => _autoSubscriber.RegisterAsync(A<User>.Ignored))
                .MustHaveHappenedTwiceExactly();
            A.CallTo(() => _autoSubscriber.LoginAsync("admin", "1234"))
                .MustHaveHappenedTwiceExactly();
            A.CallTo(() => _autoSubscriber.EarseAsync(4335))
                .MustHaveHappenedTwiceExactly();

            #endregion // Validation
        }

        #endregion // GeneratedContract_Factory_Test

        #region OnSucceed_ACK_WithFailure_Test

        [Fact]
        public async Task OnSucceed_ACK_WithFailure_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Partition(PARTITION)
                                            .Shard(SHARD)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            #region A.CallTo(() => _subscriber.LoginAsync(throw 1 time))

            int tryNumber = 0;
            A.CallTo(() => _subscriber.LoginAsync(A<string>.Ignored, A<string>.Ignored))
                .ReturnsLazily<ValueTask>(() =>
                {
                    // 3 error will be catch by Polly, the 4th one will catch outside of Polly
                    if (Interlocked.Increment(ref tryNumber) < 5)
                        throw new ApplicationException("test intensional exception");

                    return ValueTask.CompletedTask;
                });

            #endregion // A.CallTo(() => _subscriber.LoginAsync(throw 1 time))

            await SendSequenceAsync(producer);

            var consumerOptions = new ConsumerOptions
            {
                AckBehavior = AckBehavior.OnSucceed,
                MaxMessages = 4 /* detach consumer after 4 messages*/
            };
            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                         .WithOptions(o => consumerOptions)
                         .WithCancellation(cancellation)
                         .Partition(PARTITION)
                         .Shard(SHARD)
                         .WithResiliencePolicy(Policy.Handle<Exception>().RetryAsync(3, (ex, i) => _outputHelper.WriteLine($"Retry {i}")))
                         .WithLogger(_fakeLogger)
                         .Subscribe(_subscriberBridge, "CONSUMER_GROUP_1", $"TEST {DateTime.UtcNow:HH:mm:ss}");

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await subscription.Completion;

            #region Validation

            A.CallTo(() => _subscriber.RegisterAsync(A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.LoginAsync("admin", "1234"))
                        .MustHaveHappened(
                                    3 /* Polly retry */ + 1 /* error */ + 1 /* succeed */,
                                    Times.Exactly);
            A.CallTo(() => _subscriber.EarseAsync(4335))
                .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // OnSucceed_ACK_WithFailure_Test

        #region OnFinaly_ACK_WithFailure_Test

        [Fact]
        public async Task OnFinaly_ACK_WithFailure_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            .Partition(PARTITION)
                                            .Shard(SHARD)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            #region A.CallTo(() => _subscriber.LoginAsync(throw 1 time))

            int tryNumber = 0;
            A.CallTo(() => _subscriber.LoginAsync(A<string>.Ignored, A<string>.Ignored))
                .ReturnsLazily<ValueTask>(() =>
                {
                    // 3 error will be catch by Polly, the 4th one will catch outside of Polly
                    if (Interlocked.Increment(ref tryNumber) < 5)
                        throw new ApplicationException("test intensional exception");

                    return ValueTask.CompletedTask;
                });

            #endregion // A.CallTo(() => _subscriber.LoginAsync(throw 1 time))

            await SendSequenceAsync(producer);

            var consumerOptions = new ConsumerOptions
            {
                AckBehavior = AckBehavior.OnFinally,
                MaxMessages = 3 /* detach consumer after 4 messages*/
            };
            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                         .WithOptions(o => consumerOptions)
                         .WithCancellation(cancellation)
                         .Partition(PARTITION)
                         .Shard(SHARD)
                         .WithResiliencePolicy(Policy.Handle<Exception>().RetryAsync(3, (ex, i) => _outputHelper.WriteLine($"Retry {i}")))
                         .WithLogger(_fakeLogger)
                         .Subscribe(_subscriberBridge, "CONSUMER_GROUP_1", $"TEST {DateTime.UtcNow:HH:mm:ss}");

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await subscription.Completion;

            #region Validation

            A.CallTo(() => _subscriber.RegisterAsync(A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.LoginAsync("admin", "1234"))
                        .MustHaveHappened(
                                    3 /* Polly retry */ + 1 /* error */ ,
                                    Times.Exactly);
            A.CallTo(() => _subscriber.EarseAsync(4335))
                .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // OnFinaly_ACK_WithFailure_Test

        #region Manual_ACK_Test

        [Fact]
        public async Task Manual_ACK_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Partition(PARTITION)
                                            .Shard(SHARD)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            #region A.CallTo(...).ReturnsLazily(...)


            int tryNumber = 0;
            A.CallTo(() => _subscriber.RegisterAsync(A<User>.Ignored))
                    .ReturnsLazily(() => Ack.Current.AckAsync());
            A.CallTo(() => _subscriber.LoginAsync(A<string>.Ignored, A<string>.Ignored))
                .ReturnsLazily<ValueTask>(async () =>
                {
                    // 3 error will be catch by Polly, the 4th one will catch outside of Polly
                    if (Interlocked.Increment(ref tryNumber) < 5)
                        throw new ApplicationException("test intensional exception");

                    await Ack.Current.AckAsync();
                });
            A.CallTo(() => _subscriber.EarseAsync(A<int>.Ignored))
                    .ReturnsLazily(() => Ack.Current.AckAsync());

            #endregion // A.CallTo(...).ReturnsLazily(...)


            await SendSequenceAsync(producer);

            var consumerOptions = new ConsumerOptions
            {
                AckBehavior = AckBehavior.Manual,
                MaxMessages = 4 /* detach consumer after 4 messages*/
            };
            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                         .WithOptions(o => consumerOptions)
                             .WithCancellation(cancellation)
                             .Partition(PARTITION)
                             .Shard(SHARD)
                             .WithResiliencePolicy(Policy.Handle<Exception>().RetryAsync(3, (ex, i) => _outputHelper.WriteLine($"Retry {i}")))
                             .WithLogger(_fakeLogger)
                             .Subscribe(_subscriberBridge, "CONSUMER_GROUP_1", $"TEST {DateTime.UtcNow:HH:mm:ss}");

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await subscription.Completion;

            #region Validation

            A.CallTo(() => _subscriber.RegisterAsync(A<User>.Ignored))
                        .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.LoginAsync("admin", "1234"))
                        .MustHaveHappened(
                                    3 /* Polly retry */ + 1 /* error */ + 1 /* succeed */,
                                    Times.Exactly);
            A.CallTo(() => _subscriber.EarseAsync(4335))
                        .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // Manual_ACK_Test

        #region Resilience_Test

        [Fact]
        public async Task Resilience_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Partition(PARTITION)
                                            .Shard(SHARD)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            int tryNumber = 0;
            A.CallTo(() => _subscriber.RegisterAsync(A<User>.Ignored))
                    .ReturnsLazily(() => Ack.Current.AckAsync());
            A.CallTo(() => _subscriber.LoginAsync(A<string>.Ignored, A<string>.Ignored))
                .ReturnsLazily<ValueTask>(async () =>
                {
                    if (Interlocked.Increment(ref tryNumber) == 1)
                        throw new ApplicationException("test intensional exception");

                    await Ack.Current.AckAsync();
                });
            A.CallTo(() => _subscriber.EarseAsync(A<int>.Ignored))
                    .ReturnsLazily(() => Ack.Current.AckAsync());


            await SendSequenceAsync(producer);

            var consumerOptions = new ConsumerOptions
            {
                AckBehavior = AckBehavior.Manual,
                MaxMessages = 3 /* detach consumer after 3 messages*/
            };
            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                         .WithOptions(o => consumerOptions)
                             .WithCancellation(cancellation)
                             .Partition(PARTITION)
                             .Shard(SHARD)
                             .WithResiliencePolicy(Policy.Handle<Exception>().RetryAsync(3))
                             .WithLogger(_fakeLogger)
                             .Subscribe(_subscriberBridge, "CONSUMER_GROUP_1", $"TEST {DateTime.UtcNow:HH:mm:ss}");

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await subscription.Completion;

            #region Validation

            A.CallTo(() => _subscriber.RegisterAsync(A<User>.Ignored))
                        .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.LoginAsync("admin", "1234"))
                        .MustHaveHappenedTwiceExactly(); /* 1 Polly, 1 succeed */
            A.CallTo(() => _subscriber.EarseAsync(4335))
                        .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // Manual_ACK_Test

        #region Override_Test

        [Fact]
        public async Task Override_Test()
        {
            #region ISequenceOperations producer = ...

            var producerBuilder = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Partition(PARTITION)
                                            .Shard(SHARD)
                                            .WithLogger(_fakeLogger);
            ISequenceOperationsProducer producer = producerBuilder.BuildSequenceOperationsProducer();
            ISequenceOperationsProducer producerPrefix = producerBuilder
                .Override<ISequenceOperationsProducer>()
                .Environment("qa")
                .Partition("p0.")
                .Shard("p1.")
                .BuildSequenceOperationsProducer();
            ISequenceOperationsProducer producerPrefix1 = producerBuilder
                .Override<ISequenceOperationsProducer>()
                .Partition("p2.").BuildSequenceOperationsProducer();
            ISequenceOperationsProducer producerSuffix = producerBuilder
                .Override<ISequenceOperationsProducer>()
                .Partition(".s0", RouteAssignmentType.Suffix)
                .Shard(".s1", RouteAssignmentType.Suffix)
                .BuildSequenceOperationsProducer();
            ISequenceOperationsProducer producerSuffix1 = producerBuilder
                .Override<ISequenceOperationsProducer>()
                .Partition(".s2", RouteAssignmentType.Suffix)
                .BuildSequenceOperationsProducer();
            ISequenceOperationsProducer producerDynamic = producerBuilder
                .Override<ISequenceOperationsProducer>()
                .Strategy(m => (m.Environment, $"d.{m.Partition}", $"{m.Shard}.d"))
                .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            await SendSequenceAsync(producer);
            await SendSequenceAsync(producerPrefix, "p0");
            await SendSequenceAsync(producerPrefix1, "p1");
            await SendSequenceAsync(producerSuffix, "s0");
            await SendSequenceAsync(producerSuffix1, "s1");
            await SendSequenceAsync(producerDynamic, "d");

            var consumerOptions = new ConsumerOptions
            {
                AckBehavior = AckBehavior.OnSucceed,
                MaxMessages = 3 /* detach consumer after 3 messages*/
            };
            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                         .WithOptions(o => consumerOptions)
                         .WithCancellation(cancellation)
                         .Partition(PARTITION)
                         .Shard(SHARD)
                         .WithLogger(_fakeLogger)
                         .Subscribe(_subscriberBridge, "CONSUMER_GROUP_1", $"TEST {DateTime.UtcNow:HH:mm:ss}");

            await using IConsumerLifetime subscriptionPrefix = _consumerBuilder
                         .WithOptions(o => consumerOptions)
                         .WithCancellation(cancellation)
                         .Environment("qa")
                         .Partition($"p0.{PARTITION}")
                         .Shard($"p1.{SHARD}")
                         .WithLogger(_fakeLogger)
                         .Subscribe(new SequenceOperationsBridge(_subscriberPrefix), "CONSUMER_GROUP_P1", $"TEST-P1 {DateTime.UtcNow:HH:mm:ss}");

            await using IConsumerLifetime subscriptionPrefix1 = _consumerBuilder
                         .WithOptions(o => consumerOptions)
                         .WithCancellation(cancellation)
                         .Partition($"p2.{PARTITION}")
                         .Shard(SHARD)
                         .WithLogger(_fakeLogger)
                         .Subscribe(new SequenceOperationsBridge(_subscriberPrefix1), "CONSUMER_GROUP_P2", $"TEST-P2 {DateTime.UtcNow:HH:mm:ss}");

            await using IConsumerLifetime subscriptionSuffix = _consumerBuilder
                         .WithOptions(o => consumerOptions)
                         .WithCancellation(cancellation)
                         .Partition($"{PARTITION}.s0")
                         .Shard($"{SHARD}.s1")
                         .WithLogger(_fakeLogger)
                         .Subscribe(new SequenceOperationsBridge(_subscriberSuffix), "CONSUMER_GROUP_S1", $"TEST-S1 {DateTime.UtcNow:HH:mm:ss}");

            await using IConsumerLifetime subscriptionSuffix1 = _consumerBuilder
                         .WithOptions(o => consumerOptions)
                         .WithCancellation(cancellation)
                         .Partition($"{PARTITION}.s2")
                         .Shard(SHARD)
                         .WithLogger(_fakeLogger)
                         .Subscribe(new SequenceOperationsBridge(_subscriberSuffix1), "CONSUMER_GROUP_S2", $"TEST-S2 {DateTime.UtcNow:HH:mm:ss}");

            await using IConsumerLifetime subscriptionDynamic = _consumerBuilder
                         .WithOptions(o => consumerOptions)
                         .WithCancellation(cancellation)
                         .Partition($"d.{PARTITION}")
                         .Shard($"{SHARD}.d")
                         .WithLogger(_fakeLogger)
                         .Subscribe(new SequenceOperationsBridge(_subscriberDynamic), "CONSUMER_GROUP_D", $"TEST-D {DateTime.UtcNow:HH:mm:ss}");

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await Task.WhenAll(
                subscription.Completion,
                subscriptionPrefix.Completion,
                subscriptionPrefix1.Completion,
                subscriptionSuffix.Completion,
                subscriptionSuffix1.Completion);

            #region Validation

            A.CallTo(() => _subscriber.RegisterAsync(A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.LoginAsync("admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.EarseAsync(4335))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _subscriberPrefix.RegisterAsync(A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberPrefix.LoginAsync("admin", "p0"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberPrefix.EarseAsync(4335))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _subscriberPrefix1.RegisterAsync(A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberPrefix1.LoginAsync("admin", "p1"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberPrefix1.EarseAsync(4335))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _subscriberSuffix.RegisterAsync(A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberSuffix.LoginAsync("admin", "s0"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberSuffix.EarseAsync(4335))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _subscriberSuffix1.RegisterAsync(A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberSuffix1.LoginAsync("admin", "s1"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberSuffix1.EarseAsync(4335))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _subscriberDynamic.RegisterAsync(A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberDynamic.LoginAsync("admin", "d"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberDynamic.EarseAsync(4335))
                .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // Override_Test

        #region Claim_Test

        [Fact]
        public async Task Claim_Test()
        {
            ISequenceOperations otherSubscriber = A.Fake<ISequenceOperations>();

            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Partition(PARTITION)
                                            .Shard(SHARD)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            #region A.CallTo(...).ReturnsLazily(...)

            A.CallTo(() => otherSubscriber.RegisterAsync(A<User>.Ignored))
                .ReturnsLazily<ValueTask>(() =>
                {
                    throw new ApplicationException("test intensional exception");
                });
            A.CallTo(() => otherSubscriber.LoginAsync(A<string>.Ignored, A<string>.Ignored))
                    .ReturnsLazily(() => ValueTask.CompletedTask);
            A.CallTo(() => otherSubscriber.EarseAsync(A<int>.Ignored))
                    .ReturnsLazily(() => ValueTask.CompletedTask);

            #endregion // A.CallTo(...).ReturnsLazily(...)

            await SendSequenceAsync(producer);

            var consumerOptions = new ConsumerOptions
            {
                AckBehavior = AckBehavior.OnSucceed,
                MaxMessages = 3 /* detach consumer after 3 messages*/
            };
            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            var consumerPipe = _consumerBuilder
                         .WithOptions(o => consumerOptions)
                             .WithCancellation(cancellation)
                             .Partition(PARTITION)
                             .Shard(SHARD)
                             .WithResiliencePolicy(Policy.Handle<Exception>().RetryAsync(3))
                             .WithLogger(_fakeLogger);

            await using IConsumerLifetime otherSubscription = consumerPipe
                             .Subscribe(new SequenceOperationsBridge(otherSubscriber), "CONSUMER_GROUP_1", $"TEST Other {DateTime.UtcNow:HH:mm:ss}");

            await otherSubscription.Completion;

            await using IConsumerLifetime subscription = consumerPipe
                             .Subscribe(new SequenceOperationsBridge(_subscriber), "CONSUMER_GROUP_1", $"TEST {DateTime.UtcNow:HH:mm:ss}");

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await subscription.Completion;

            #region Validation

            A.CallTo(() => otherSubscriber.RegisterAsync(A<User>.Ignored))
                        .MustHaveHappened(
                                    (3 /* Polly retry */ + 1 /* throw */ ) * 3 /* disconnect after 3 messaged */ ,
                                    Times.Exactly);
            A.CallTo(() => _subscriber.RegisterAsync(A<User>.Ignored))
                        .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.LoginAsync("admin", "1234"))
                        .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.EarseAsync(4335))
                        .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // Claim_Test

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


        ~EndToEndTests()
        {
            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            string[] keys =
                {
                    $"{PARTITION}:{SHARD}",
                    $"Test:{PARTITION}:{SHARD}",
                    $"qa:p0.{PARTITION}:p1.{SHARD}",
                    $"p2.{PARTITION}:{SHARD}",
                    $"{PARTITION}.s0:{SHARD}.s1",
                    $"{PARTITION}.s2:{SHARD}",
                    $"d.{PARTITION}:{SHARD}.d",
                };
            IConnectionMultiplexer conn = RedisClientFactory.CreateProviderBlocking(
                                                _fakeLogger,
                                                cfg => cfg.AllowAdmin = true);
            IDatabaseAsync db = conn.GetDatabase();
            foreach (string key in keys)
            {
                db.KeyDeleteAsync(key, CommandFlags.DemandMaster).Wait();
            }

        }

        #endregion // Dispose pattern
    }
}
