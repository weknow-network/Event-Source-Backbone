using System.Diagnostics;
using System.Text.Json;

using EventSourcing.Backbone.Building;
using EventSourcing.Backbone.Channels.RedisProvider;
using EventSourcing.Backbone.Enums;
using EventSourcing.Backbone.Tests.Entities;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Polly;

using Xunit;
using Xunit.Abstractions;

using static EventSourcing.Backbone.EventSourceConstants;

#pragma warning disable S3881 // "IDisposable" should be implemented correctly

// docker run -p 6379:6379 -it --rm --name redis-evt-src redislabs/rejson:latest

namespace EventSourcing.Backbone.Tests
{
    /// <summary>
    /// The end to end tests.
    /// </summary>
    public class EndToEndTests : TestsBase
    {
        private readonly ISequenceOperationsConsumer _subscriber1 = A.Fake<ISequenceOperationsConsumer>();
        private readonly ISequenceOperationsConsumer _subscriber2 = A.Fake<ISequenceOperationsConsumer>();
        private readonly ISequenceOperationsConsumer _subscriber3 = A.Fake<ISequenceOperationsConsumer>();
        private readonly SequenceOperationsConsumerBridge _subscriberBridge;
        private readonly ISequenceOperationsConsumer _autoSubscriber = A.Fake<ISequenceOperationsConsumer>();
        private readonly ISequenceOperationsConsumer _subscriberPrefix = A.Fake<ISequenceOperationsConsumer>();
        private readonly ISequenceOperationsConsumer _subscriberPrefix1 = A.Fake<ISequenceOperationsConsumer>();
        private readonly ISequenceOperationsConsumer _subscriberSuffix = A.Fake<ISequenceOperationsConsumer>();
        private readonly ISequenceOperationsConsumer _subscriberSuffix1 = A.Fake<ISequenceOperationsConsumer>();
        private readonly ISequenceOperationsConsumer _subscriberDynamic = A.Fake<ISequenceOperationsConsumer>();
        private readonly IProducerStoreStrategyBuilder _producerBuilder;
        private readonly IConsumerStoreStrategyBuilder _consumerBuilder;
        private readonly IEventFlowStage1Consumer _stage1Consumer = A.Fake<IEventFlowStage1Consumer>();
        private readonly IEventFlowStage2Consumer _stage2Consumer = A.Fake<IEventFlowStage2Consumer>();

        private readonly string ENV = $"test";
        protected override string URI { get; } = $"{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}:{Guid.NewGuid():N}";

        private static readonly User USER = new User { Eracure = new Personal { Name = "mike", GovernmentId = "A25" }, Comment = "Do it" };

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="EndToEndTests"/> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
        /// <param name="producerChannelBuilder">The producer channel builder.</param>
        /// <param name="consumerChannelBuilder">The consumer channel builder.</param>
        public EndToEndTests(
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
            _consumerBuilder = consumerChannelBuilder?.Invoke(consumerBuilder, _fakeLogger) ?? consumerBuilder;

            A.CallTo(() => _subscriber1.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                    .ReturnsLazily(() =>
                    {
                        Metadata meta = ConsumerContext.Context;
                        if (string.IsNullOrEmpty(meta.EventKey))
                            return ValueTask.FromException(new EventSourcingException("Event Key is missing"));
                        return ValueTask.CompletedTask;
                    });
            A.CallTo(() => _subscriber1.LoginAsync(A<ConsumerContext>.Ignored, A<string>.Ignored, A<string>.Ignored))
                    .ReturnsLazily(() => Delay());
            A.CallTo(() => _subscriber1.EarseAsync(A<ConsumerContext>.Ignored, A<int>.Ignored))
                    .ReturnsLazily(() => Delay());

            A.CallTo(() => _subscriber2.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                    .ReturnsLazily(() =>
                    {
                        Metadata meta = ConsumerContext.Context;
                        if (string.IsNullOrEmpty(meta.EventKey))
                            return ValueTask.FromException(new EventSourcingException("Event Key is missing"));
                        return ValueTask.CompletedTask;
                    });
            A.CallTo(() => _subscriber2.LoginAsync(A<ConsumerContext>.Ignored, A<string>.Ignored, A<string>.Ignored))
                    .ReturnsLazily(() => Delay());
            A.CallTo(() => _subscriber2.EarseAsync(A<ConsumerContext>.Ignored, A<int>.Ignored))
                    .ReturnsLazily(() => Delay());

            A.CallTo(() => _subscriber3.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                    .ReturnsLazily(() =>
                    {
                        Metadata meta = ConsumerContext.Context;
                        if (string.IsNullOrEmpty(meta.EventKey))
                            return ValueTask.FromException(new EventSourcingException("Event Key is missing"));
                        return ValueTask.CompletedTask;
                    });
            A.CallTo(() => _subscriber3.LoginAsync(A<ConsumerContext>.Ignored, A<string>.Ignored, A<string>.Ignored))
                    .ReturnsLazily(() => Delay());
            A.CallTo(() => _subscriber3.EarseAsync(A<ConsumerContext>.Ignored, A<int>.Ignored))
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

            _subscriberBridge = new SequenceOperationsConsumerBridge(_subscriber1);
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

        #region Environment_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task Environment_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment(ENV)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            await SendSequenceAsync(producer);

            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, 3, AckBehavior.OnSucceed))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .Group("CONSUMER_GROUP_1")
                         .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                         .Subscribe(_subscriberBridge);

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            var sw = Stopwatch.StartNew();
            await subscription.Completion;
            sw.Stop();
            _outputHelper.WriteLine($"Consume Duration = {sw.Elapsed:mm\\:ss\\.ff}");

            #region Validation

            A.CallTo(() => _subscriber1.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber1.LoginAsync(A<ConsumerContext>.Ignored, "admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber1.EarseAsync(A<ConsumerContext>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // Environment_Test

        #region MultiTargets_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task MultiTargets_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment(ENV)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            await SendSequenceAsync(producer);

            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, 3, AckBehavior.OnSucceed))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .Group("CONSUMER_GROUP_1")
                         .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                         .SubscribeSequenceOperationsConsumer(_subscriber1, _subscriber2, _subscriber3);

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            var sw = Stopwatch.StartNew();
            await subscription.Completion;
            sw.Stop();
            _outputHelper.WriteLine($"Consume Duration = {sw.Elapsed:mm\\:ss\\.ff}");

            #region Validation

            A.CallTo(() => _subscriber1.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber1.LoginAsync(A<ConsumerContext>.Ignored, "admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber1.EarseAsync(A<ConsumerContext>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _subscriber2.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber2.LoginAsync(A<ConsumerContext>.Ignored, "admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber2.EarseAsync(A<ConsumerContext>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _subscriber3.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber3.LoginAsync(A<ConsumerContext>.Ignored, "admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber3.EarseAsync(A<ConsumerContext>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // MultiTargets_Test

        #region MultiSubscriber_MultiTargets_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task MultiSubscriber_MultiTargets_Test()
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

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            var consumerBuilder = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, 3, AckBehavior.OnSucceed))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .Group("CONSUMER_GROUP_1")
                         .Name($"TEST {DateTime.UtcNow:HH:mm:ss}");
            await using IConsumerLifetime subscription1 =
                         consumerBuilder.SubscribeSequenceOperationsConsumer(_subscriber1);
            await using IConsumerLifetime subscription2 =
                         consumerBuilder.Group("Other-Group")
                                        .SubscribeSequenceOperationsConsumer(_subscriber2, _subscriber3);

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await SendSequenceAsync(producer);

            var sw = Stopwatch.StartNew();
            await subscription1.Completion;
            await subscription2.Completion;
            sw.Stop();
            _outputHelper.WriteLine($"Consume Duration = {sw.Elapsed:mm\\:ss\\.ff}");

            #region Validation

            A.CallTo(() => _subscriber1.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber1.LoginAsync(A<ConsumerContext>.Ignored, "admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber1.EarseAsync(A<ConsumerContext>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _subscriber2.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber2.LoginAsync(A<ConsumerContext>.Ignored, "admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber2.EarseAsync(A<ConsumerContext>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _subscriber3.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber3.LoginAsync(A<ConsumerContext>.Ignored, "admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber3.EarseAsync(A<ConsumerContext>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // MultiSubscriber_MultiTargets_Test

        #region PartialConsumer_Strict_Succeed_Test

        [Fact(Timeout = TIMEOUT, Skip = "Pending feature https://github.com/users/bnayae/projects/8/views/1?query=is%3Aopen+sort%3Aupdated-desc&pane=issue&itemId=34366135")]
        public async Task PartialConsumer_Strict_Succeed_Test()
        {
            #region ISequenceOperations producer = ...

            IEventFlowProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment(ENV)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildEventFlowProducer();

            #endregion // ISequenceOperations producer = ...

            var sw = Stopwatch.StartNew();

            var p = new Person(100, "bnaya");
            await producer.Stage1Async(p, "ABC");
            await producer.Stage2Async(p.ToJson(), "ABC".ToJson());

            var snapshot = sw.Elapsed;
            _outputHelper.WriteLine($"Produce = {snapshot:mm\\:ss\\.ff}");
            snapshot = sw.Elapsed;

            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            var consumerBuilder = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, 2, AckBehavior.OnSucceed))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .Group("CONSUMER_GROUP_X_1")
                         .Name($"TEST {DateTime.UtcNow:HH:mm:ss}");
            await using IConsumerLifetime subscription1 =
                         consumerBuilder.SubscribeEventFlowStage1Consumer(_stage1Consumer);
            await using IConsumerLifetime subscription2 =
                         consumerBuilder.SubscribeEventFlowStage2Consumer(_stage2Consumer);

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await subscription1.Completion;
            await subscription2.Completion;

            snapshot = sw.Elapsed - snapshot;
            _outputHelper.WriteLine($"Consumed = {snapshot:mm\\:ss\\.ff}");

            #region Validation

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log" && call.GetArgument<LogLevel>(0) == LogLevel.Critical).MustNotHaveHappened();

            A.CallTo(() => _stage1Consumer.Stage1Async(A<ConsumerContext>.Ignored, A<Person>.Ignored, A<string>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _stage2Consumer.Stage2Async(A<ConsumerContext>.Ignored, A<Person>.Ignored, A<JsonElement>.Ignored))
                .MustHaveHappenedOnceExactly();

            #endregion // Validation

        }

        #endregion // PartialConsumer_Strict_Succeed_Test

        #region PartialConsumer_Strict_Fail_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task PartialConsumer_Strict_Fail_Test()
        {
            #region ISequenceOperations producer = ...

            IEventFlowProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment(ENV)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildEventFlowProducer();

            #endregion // ISequenceOperations producer = ...

            var p = new Person(100, "bnaya");
            await producer.Stage1Async(p, "ABC");
            await producer.Stage2Async(p.ToJson(), "ABC".ToJson());

            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, 2, AckBehavior.OnSucceed) with { PartialBehavior = PartialConsumerBehavior.ThrowIfNotHandled })
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .Group("CONSUMER_GROUP_X_1")
                         .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                         .SubscribeEventFlowStage2Consumer(_stage2Consumer);

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await subscription.Completion;

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log" && call.GetArgument<LogLevel>(0) == LogLevel.Critical).MustHaveHappened();
        }

        #endregion // PartialConsumer_Strict_Fail_Test

        #region PartialConsumer_Allow_Test

        [Fact(Timeout = TIMEOUT, Skip = "Pending feature https://github.com/users/bnayae/projects/8/views/1?query=is%3Aopen+sort%3Aupdated-desc&pane=issue&itemId=34366135")]
        public async Task PartialConsumer_Allow_Test()
        {
            uint times = 20;

            #region ISequenceOperations producer = ...

            IEventFlowProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment(ENV)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildEventFlowProducer();

            #endregion // ISequenceOperations producer = ...

            for (int i = 0; i < times; i++)
            {
                var p = new Person(100 + i, "bnaya");
                _outputHelper.WriteLine($"Cycle {i}");

                await producer.Stage1Async(p, "ABC");
                await producer.Stage2Async(p.ToJson(), "ABC".ToJson());
            }

            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, times * 2, AckBehavior.OnSucceed, PartialConsumerBehavior.Loose))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .Group("CONSUMER_GROUP_X_1")
                         .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                         .SubscribeEventFlowStage2Consumer(_stage2Consumer);

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await subscription.Completion;

            #region Validation

            A.CallTo(() => _stage2Consumer.Stage2Async(A<ConsumerContext>.Ignored, A<Person>.Ignored, A<JsonElement>.Ignored))
                .MustHaveHappened((int)times, Times.Exactly);

            #endregion // Validation
        }

        #endregion // PartialConsumer_Allow_Test

        #region Receiver_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task Receiver_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment(ENV)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            EventKeys keys = await SendSequenceAsync(producer);

            CancellationToken cancellation = GetCancellationToken();

            #region IConsumerReceiver receiver = ...

            IConsumerReceiver receiver = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .BuildReceiver();

            #endregion // IConsumerReceiver receiver = ...

            AnnouncementData? res0 = await receiver.GetByIdAsync(keys[0]);
            Assert.NotNull(res0);
            var res1 = await receiver.GetByIdAsync(keys[1]);
            var hasEmail = res1.Data.TryGet("email", out string? email);
            Assert.True(hasEmail);
            Assert.Equal("admin", email);
            var res2 = await receiver.GetByIdAsync(keys[2]);
            var hasId = res2.Data.TryGet("id", out int id);
            Assert.Equal(4335, id);
            Assert.True(hasId);
        }

        #endregion // Receiver_Test

        #region Receiver_Json_Test

        private record TestEmailPass(string email, string password);


        [Fact(Timeout = TIMEOUT)]
        public async Task Receiver_Json_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment(ENV)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            EventKeys keys = await SendSequenceAsync(producer);

            CancellationToken cancellation = GetCancellationToken();

            #region IConsumerReceiver receiver = ...

            IConsumerReceiver receiver = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .BuildReceiver();

            #endregion // IConsumerReceiver receiver = ...

            var res0 = await receiver.GetJsonByIdAsync(keys[0]);
            _outputHelper.WriteLine(res0.AsIndentString());
            Assert.True(res0.TryGetProperty("user", out JsonElement uj0));
            var u0 = JsonSerializer.Deserialize<User>(uj0.GetRawText(), SerializerOptionsWithIndent);
            Assert.Equal("A25", u0?.Eracure?.GovernmentId);
            Assert.Equal("mike", u0?.Eracure?.Name);

            var res1 = await receiver.GetJsonByIdAsync(keys[1]);
            _outputHelper.WriteLine(res1.AsIndentString());
            var p1 = JsonSerializer.Deserialize<TestEmailPass>(res1.GetRawText(), SerializerOptionsWithIndent);
            Assert.Equal("admin", p1?.email);
            Assert.Equal("1234", p1?.password);

            var res2 = await receiver.GetJsonByIdAsync(keys[2]);
            _outputHelper.WriteLine(res2.AsIndentString());
            var pj2 = res2.GetProperty("id");
            Assert.Equal(4335, pj2.GetInt32());
        }

        #endregion // Receiver_Json_Test

        #region Receiver_ChangeEnvironment_AfterBuild_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task Receiver_ChangeEnvironment_AfterBuild()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment("FakeTest")
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .Environment(ENV)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            EventKeys keys = await SendSequenceAsync(producer);

            CancellationToken cancellation = GetCancellationToken();

            #region IConsumerReceiver receiver = ...

            IConsumerReceiver receiver = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o))
                         .WithCancellation(cancellation)
                         .Environment("DemoTest")
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .BuildReceiver()
                         .Environment(ENV);

            #endregion // IConsumerReceiver receiver = ...

            var res0 = await receiver.GetByIdAsync(keys[0]);
            var res1 = await receiver.GetByIdAsync(keys[1]);
            var hasEmail = res1.Data.TryGet("email", out string? email);
            Assert.Equal("admin", email);
            var res2 = await receiver.GetByIdAsync(keys[2]);
            var hasId = res2.Data.TryGet("id", out int id);
            Assert.Equal(4335, id);
            Assert.NotNull(res0);
            Assert.True(hasEmail);
            Assert.True(hasId);
        }

        #endregion // Receiver_ChangeEnvironment_AfterBuild

        #region Receiver_ChangeEnvironment_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task Receiver_ChangeEnvironment_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment("FakeTest")
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .Environment(ENV)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            EventKeys keys = await SendSequenceAsync(producer);

            CancellationToken cancellation = GetCancellationToken();

            #region IConsumerReceiver receiver = ...

            IConsumerReceiver receiver = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o))
                         .WithCancellation(cancellation)
                         .Environment("DemoTest")
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .Environment(ENV)
                         .BuildReceiver();

            #endregion // IConsumerReceiver receiver = ...

            var res0 = await receiver.GetByIdAsync(keys[0]);
            var res1 = await receiver.GetByIdAsync(keys[1]);
            var hasEmail = res1.Data.TryGet("email", out string? email);
            Assert.Equal("admin", email);
            var res2 = await receiver.GetByIdAsync(keys[2]);
            var hasId = res2.Data.TryGet("id", out int id);
            Assert.Equal(4335, id);
            Assert.NotNull(res0);
            Assert.True(hasEmail);
            Assert.True(hasId);
        }

        #endregion // Receiver_ChangeEnvironment_Test

        #region Iterator_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task Iterator_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment(ENV)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            await SendSequenceAsync(producer);

            CancellationToken cancellation = GetCancellationToken();

            #region IConsumerIterator iterator = ...

            IConsumerIterator iterator = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .BuildIterator();

            #endregion // IConsumerIterator iterator = ...

            int i = 0;
            await foreach (AnnouncementData announcement in iterator.GetAsyncEnumerable().WithCancellation(cancellation))
            {
                if (i == 0)
                {
                    Assert.True(announcement.Data.TryGet("user", out User? user));
                    Assert.Equal(USER.Eracure?.Name, user?.Eracure?.Name);

                }
                if (i == 1)
                {
                    Assert.True(announcement.Data.TryGet("email", out string? email));
                    Assert.Equal("admin", email);
                }
                if (i == 2)
                {
                    Assert.True(announcement.Data.TryGet("id", out int id));
                    Assert.Equal(4335, id);
                }
                i++;
            }
        }

        #endregion // Iterator_Test

        #region Iterator_Cancellation_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task Iterator_Cancellation_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment(ENV)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            await SendSequenceAsync(producer);

            var cts = new CancellationTokenSource();
            CancellationToken globalCancellation = GetCancellationToken();
            CancellationTokenSource linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, globalCancellation);
            CancellationToken cancellation = linkedCancellation.Token;

            #region IConsumerIterator iterator = ...

            IConsumerIterator iterator = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .BuildIterator();

            #endregion // IConsumerIterator iterator = ...

            int i = 0;
            await foreach (AnnouncementData announcement in iterator.GetAsyncEnumerable().WithCancellation(cancellation))
            {
                if (i == 0)
                {
                    Assert.True(announcement.Data.TryGet("user", out User? user));
                    Assert.Equal(USER.Eracure?.Name, user?.Eracure?.Name);
                    cts.Cancel();
                }
                else if (i == 1)
                {
                    throw new OperationCanceledException("Should have been canceled");
                }
                else throw new EventSourcingException("Should have been canceled");

                i++;
            }

            await foreach (AnnouncementData announcement in iterator.GetAsyncEnumerable(cancellationToken: cancellation))
            {
                if (i == 0)
                {
                    Assert.True(announcement.Data.TryGet("user", out User? user));
                    Assert.Equal(USER.Eracure?.Name, user?.Eracure?.Name);
                    cts.Cancel();
                }
                if (i == 1)
                {
                    throw new OperationCanceledException("Should have been canceled");
                }
                i++;
            }
        }

        #endregion // Iterator_Cancellation_Test

        #region Iterator_Json_NoMeta_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task Iterator_Json_NoMeta_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment(ENV)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            await SendSequenceAsync(producer);

            CancellationToken cancellation = GetCancellationToken();

            #region IConsumerIterator iterator = ...

            IConsumerIterator iterator = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .BuildIterator();

            #endregion // IConsumerIterator iterator = ...

            int i = 0;
            var options = new ConsumerAsyncEnumerableJsonOptions { IgnoreMetadata = true };
            await foreach (JsonElement json in iterator.GetJsonAsyncEnumerable(options).WithCancellation(cancellation))
            {
                _outputHelper.WriteLine(json.AsIndentString());
                if (i == 0)
                {
                    Assert.False(json.TryGetProperty("__meta__", out _));
                    Assert.True(json.TryGetProperty("user", out JsonElement uj0));
                    var u0 = JsonSerializer.Deserialize<User>(uj0.GetRawText(), SerializerOptionsWithIndent);
                    Assert.Equal("A25", u0?.Eracure?.GovernmentId);
                    Assert.Equal("mike", u0?.Eracure?.Name);
                }
                if (i == 1)
                {
                    Assert.False(json.TryGetProperty("__meta__", out _));
                    var p1 = JsonSerializer.Deserialize<TestEmailPass>(json.GetRawText(), SerializerOptionsWithIndent);
                    Assert.Equal("admin", p1?.email);
                    Assert.Equal("1234", p1?.password);
                }
                if (i == 2)
                {
                    Assert.False(json.TryGetProperty("__meta__", out _));
                    var pj2 = json.GetProperty("id");
                    Assert.Equal(4335, pj2.GetInt32());
                }
                i++;
            }
        }

        #endregion // Iterator_Json_NoMeta_Test

        #region Iterator_Json_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task Iterator_Json_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment(ENV)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            await SendSequenceAsync(producer);

            CancellationToken cancellation = GetCancellationToken();

            #region IConsumerIterator iterator = ...

            IConsumerIterator iterator = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .BuildIterator();

            #endregion // IConsumerIterator iterator = ...

            int i = 0;
            await foreach (JsonElement json in iterator.GetJsonAsyncEnumerable().WithCancellation(cancellation))
            {
                _outputHelper.WriteLine(json.AsIndentString());
                if (i == 0)
                {
                    Assert.True(json.TryGetProperty("__meta__", out _));
                    Assert.True(json.TryGetProperty("user", out JsonElement uj0));
                    var u0 = JsonSerializer.Deserialize<User>(uj0.GetRawText(), SerializerOptionsWithIndent);
                    Assert.Equal("A25", u0?.Eracure?.GovernmentId);
                    Assert.Equal("mike", u0?.Eracure?.Name);
                }
                if (i == 1)
                {
                    Assert.True(json.TryGetProperty("__meta__", out _));
                    var p1 = JsonSerializer.Deserialize<TestEmailPass>(json.GetRawText(), SerializerOptionsWithIndent);
                    Assert.Equal("admin", p1?.email);
                    Assert.Equal("1234", p1?.password);
                }
                if (i == 2)
                {
                    Assert.True(json.TryGetProperty("__meta__", out _));
                    var pj2 = json.GetProperty("id");
                    Assert.Equal(4335, pj2.GetInt32());
                }
                i++;
            }
        }

        #endregion // Iterator_Json_Test

        #region Iterator_WithFilter_Json_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task Iterator_WithFilter_Json_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder.Environment(ENV).Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            await SendSequenceAsync(producer);

            CancellationToken cancellation = GetCancellationToken();

            #region IConsumerIterator iterator = ...

            IConsumerIterator iterator = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .BuildIterator();

            #endregion // IConsumerIterator iterator = ...

            int i = 0;
            var options = new ConsumerAsyncEnumerableJsonOptions
            {
                OperationFilter = meta => meta.Signature.Operation is (nameof(ISequenceOperationsConsumer.LoginAsync)) or
                                          (nameof(ISequenceOperationsConsumer.EarseAsync))
            };
            await foreach (JsonElement json in iterator.GetJsonAsyncEnumerable(options).WithCancellation(cancellation))
            {
                _outputHelper.WriteLine(json.AsIndentString());
                if (i == 0)
                {
                    var p1 = JsonSerializer.Deserialize<TestEmailPass>(json.GetRawText(), SerializerOptionsWithIndent);
                    Assert.Equal("admin", p1?.email);
                    Assert.Equal("1234", p1?.password);
                }
                else if (i == 1)
                {
                    var pj2 = json.GetProperty("id");
                    Assert.Equal(4335, pj2.GetInt32());
                }
                else throw new EventSourcingException("Should have been filtered");
                i++;
            }
        }

        #endregion // Iterator_WithFilter_Json_Test

        #region Iterator_MapByType_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task Iterator_MapByType_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment(ENV)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            await SendSequenceAsync(producer);

            CancellationToken cancellation = GetCancellationToken();

            #region IConsumerIterator<ISequenceOperationsConsumer_EntityFamily> iterator

            var iterator = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .BuildIterator()
                         .SpecializeSequenceOperationsConsumer();

            #endregion // IConsumerIterator<ISequenceOperationsConsumer_EntityFamily> iterator

            int i = 0;
            await foreach (var item in
                                iterator.GetAsyncEnumerable<Entities.Generated.SequenceOperations.LoginAsync_V0_String_String>()
                                    .WithCancellation(cancellation))
            {
                Assert.True(i < 1);
                Assert.Equal("admin", item.email);
                Assert.Equal("1234", item.password);

                i++;
            }
        }

        #endregion // Iterator_MapByType_Test

        #region Iterator_MapByTyIterator_MapByType_WithExtension_Testpe_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task Iterator_MapByType_WithExtension_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment(ENV)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            await SendSequenceAsync(producer);

            CancellationToken cancellation = GetCancellationToken();

            #region IConsumerIterator<ISequenceOperationsConsumer_EntityFamily> iterator = ...

            var iterator = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .BuildIterator()
                         .Specialize(SequenceOperationsConsumerEntityMapper.Default);

            #endregion // IConsumerIterator<ISequenceOperationsConsumer_EntityFamily> iterator = ...

            int i = 0;
            await foreach (var item in iterator.GetAsyncEnumerable<Entities.Generated.SequenceOperations.LoginAsync_V0_String_String>()
                            .WithCancellation(cancellation))
            {
                Assert.True(i < 1);
                Assert.Equal("admin", item.email);
                Assert.Equal("1234", item.password);

                i++;
            }
        }

        #endregion // Iterator_MapByType_WithExtension_Test

        #region Until_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task Until_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            .Environment(ENV)
                                            //.WithOptions(producerOption)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            await SendSequenceAsync(producer);

            await Task.Delay(100); // DateTime is not so accurate

            CancellationToken cancellation = GetCancellationToken();

            await Task.Delay(100); // DateTime is not so accurate
            await SendSequenceAsync(producer); // should be ignored

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, 3, AckBehavior.OnSucceed) with { FetchUntilDateOrEmpty = DateTimeOffset.UtcNow })
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .Group("CONSUMER_GROUP_1")
                         .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                         .Subscribe(_subscriberBridge);

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)


            await subscription.Completion;

            #region Validation

            A.CallTo(() => _subscriber1.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber1.LoginAsync(A<ConsumerContext>.Ignored, "admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber1.EarseAsync(A<ConsumerContext>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // Until_Test

        #region GeneratedContract_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task GeneratedContract_Test()
        {
            #region ISequenceOperations producer1 = ...

            ISequenceOperationsProducer producer1 = _producerBuilder
                                            .Environment(ENV)
                                            //.WithOptions(producerOption)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer1 = ...

            #region ISequenceOperations producer2 = ...

            IProducerSequenceOperations producer2 = _producerBuilder
                                            .Environment(ENV)
                                            //.WithOptions(producerOption)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .Build(ProducerSequenceOperationsBridgePipeline.Create);

            #endregion // ISequenceOperations producer2 = ...

            await SendSequenceAsync(producer1);
            await SendSequenceAsync(producer2);

            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, 6, AckBehavior.OnSucceed)) /* detach consumer after 6 messages*/
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .Group("CONSUMER_GROUP_1")
                         .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")

                         .Subscribe(new SequenceOperationsConsumerBridge(_autoSubscriber));

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await subscription.Completion;

            #region Validation

            A.CallTo(() => _autoSubscriber.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                .MustHaveHappenedTwiceExactly();
            A.CallTo(() => _autoSubscriber.LoginAsync(A<ConsumerContext>.Ignored, "admin", "1234"))
                .MustHaveHappenedTwiceExactly();
            A.CallTo(() => _autoSubscriber.EarseAsync(A<ConsumerContext>.Ignored, 4335))
                .MustHaveHappenedTwiceExactly();

            #endregion // Validation
        }

        #endregion // GeneratedContract_Test

        #region Resilience_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task Resilience_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            .Environment(ENV)
                                            //.WithOptions(producerOption)
                                            .Uri(URI)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            int tryNumber = 0;
            A.CallTo(() => _subscriber1.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                    .ReturnsLazily(() => Ack.Current.AckAsync());
            A.CallTo(() => _subscriber1.LoginAsync(A<ConsumerContext>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .ReturnsLazily<ValueTask>(async () =>
                {
                    if (Interlocked.Increment(ref tryNumber) == 1)
                        throw new ApplicationException("test intensional exception");

                    await Ack.Current.AckAsync();
                });
            A.CallTo(() => _subscriber1.EarseAsync(A<ConsumerContext>.Ignored, A<int>.Ignored))
                    .ReturnsLazily(() => Ack.Current.AckAsync());


            await SendSequenceAsync(producer);

            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, 3, AckBehavior.Manual))
                             .WithCancellation(cancellation)
                             .Environment(ENV)
                             .Uri(URI)
                             .WithResiliencePolicy(Policy.Handle<Exception>().RetryAsync(3))
                             .WithLogger(_fakeLogger)
                             .Group("CONSUMER_GROUP_1")
                             .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                             .Subscribe(_subscriberBridge);

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await subscription.Completion;

            #region Validation

            A.CallTo(() => _subscriber1.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                        .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber1.LoginAsync(A<ConsumerContext>.Ignored, "admin", "1234"))
                        .MustHaveHappenedTwiceExactly(); /* 1 Polly, 1 succeed */
            A.CallTo(() => _subscriber1.EarseAsync(A<ConsumerContext>.Ignored, 4335))
                        .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // Manual_ACK_Test

        #region Override_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task Override_Test()
        {
            #region ISequenceOperations producer = ...

            var producerBuilder = _producerBuilder
                                            .Environment(ENV)
                                            //.WithOptions(producerOption)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger);
            ISequenceOperationsProducer producer = producerBuilder.BuildSequenceOperationsProducer();
            ISequenceOperationsProducer producerPrefix = producerBuilder
                .Specialize<ISequenceOperationsProducer>()
                .Environment("test-override")
                .Uri("p0.")
                .BuildSequenceOperationsProducer();
            ISequenceOperationsProducer producerPrefix1 = producerBuilder
                .Specialize<ISequenceOperationsProducer>()
                .Uri("p2.").BuildSequenceOperationsProducer();
            ISequenceOperationsProducer producerSuffix = producerBuilder
                .Specialize<ISequenceOperationsProducer>()
                .Uri(".s0", RouteAssignmentType.Suffix)
                .BuildSequenceOperationsProducer();
            ISequenceOperationsProducer producerSuffix1 = producerBuilder
                .Specialize<ISequenceOperationsProducer>()
                .Uri(".s2", RouteAssignmentType.Suffix)
                .BuildSequenceOperationsProducer();
            ISequenceOperationsProducer producerDynamic = producerBuilder.Environment("Fake Env")
                .Specialize<ISequenceOperationsProducer>()
                .Strategy(m => (ENV, $"d.{m.Uri}"))
                .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            await SendSequenceAsync(producer);
            await SendSequenceAsync(producerPrefix, "p0");
            await SendSequenceAsync(producerPrefix1, "p1");
            await SendSequenceAsync(producerSuffix, "s0");
            await SendSequenceAsync(producerSuffix1, "s1");
            await SendSequenceAsync(producerDynamic, "d");

            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, 3, AckBehavior.OnSucceed))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .Group("CONSUMER_GROUP_1")
                         .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                         .Subscribe(_subscriberBridge);

            await using IConsumerLifetime subscriptionPrefix = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, 3, AckBehavior.OnSucceed))
                         .WithCancellation(cancellation)
                         .Environment("test-override")
                         .Uri($"p0.{URI}")
                         .WithLogger(_fakeLogger)
                         .Group("CONSUMER_GROUP_1")
                         .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                         .Subscribe(new SequenceOperationsConsumerBridge(_subscriberPrefix));

            await using IConsumerLifetime subscriptionPrefix1 = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, 3, AckBehavior.OnSucceed))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri($"p2.{URI}")
                         .WithLogger(_fakeLogger)
                         .Group("CONSUMER_GROUP_1")
                         .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                         .Subscribe(new SequenceOperationsConsumerBridge(_subscriberPrefix1));

            await using IConsumerLifetime subscriptionSuffix = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, 3, AckBehavior.OnSucceed))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri($"{URI}.s0")
                         .WithLogger(_fakeLogger)
                         .Group("CONSUMER_GROUP_1")
                         .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                         .Subscribe(new SequenceOperationsConsumerBridge(_subscriberSuffix));

            await using IConsumerLifetime subscriptionSuffix1 = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, 3, AckBehavior.OnSucceed))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri($"{URI}.s2")
                         .WithLogger(_fakeLogger)
                         .Group("CONSUMER_GROUP_1")
                         .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                         .Subscribe(new SequenceOperationsConsumerBridge(_subscriberSuffix1));

            await using IConsumerLifetime subscriptionDynamic = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, 3, AckBehavior.OnSucceed))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri($"d.{URI}")
                         .WithLogger(_fakeLogger)
                         .Group("CONSUMER_GROUP_D")
                         .Name($"TEST_D {DateTime.UtcNow:HH:mm:ss}")
                         .Subscribe(new SequenceOperationsConsumerBridge(_subscriberDynamic));

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await Task.WhenAll(
                subscription.Completion,
                subscriptionPrefix.Completion,
                subscriptionPrefix1.Completion,
                subscriptionSuffix.Completion,
                subscriptionSuffix1.Completion,
                subscriptionDynamic.Completion);

            #region Validation

            A.CallTo(() => _subscriber1.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber1.LoginAsync(A<ConsumerContext>.Ignored, "admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber1.EarseAsync(A<ConsumerContext>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _subscriberPrefix.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberPrefix.LoginAsync(A<ConsumerContext>.Ignored, "admin", "p0"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberPrefix.EarseAsync(A<ConsumerContext>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _subscriberPrefix1.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberPrefix1.LoginAsync(A<ConsumerContext>.Ignored, "admin", "p1"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberPrefix1.EarseAsync(A<ConsumerContext>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _subscriberSuffix.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberSuffix.LoginAsync(A<ConsumerContext>.Ignored, "admin", "s0"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberSuffix.EarseAsync(A<ConsumerContext>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _subscriberSuffix1.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberSuffix1.LoginAsync(A<ConsumerContext>.Ignored, "admin", "s1"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberSuffix1.EarseAsync(A<ConsumerContext>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _subscriberDynamic.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberDynamic.LoginAsync(A<ConsumerContext>.Ignored, "admin", "d"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriberDynamic.EarseAsync(A<ConsumerContext>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // Override_Test

        #region Claim_Test

        [Fact] // (Timeout = TIMEOUT)]
        public async Task Claim_Test()
        {
            var CONSUMER_GROUP = "CONSUMER_GROUP_1";
            ISequenceOperationsConsumer otherSubscriber = A.Fake<ISequenceOperationsConsumer>();

            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            .Environment(ENV)
                                            //.WithOptions(producerOption)
                                            .Uri(URI)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            #region A.CallTo(...).ReturnsLazily(...)

            A.CallTo(() => otherSubscriber.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                .ReturnsLazily<ValueTask>(() =>
                {
                    throw new ApplicationException("test intensional exception");
                });
            A.CallTo(() => otherSubscriber.LoginAsync(A<ConsumerContext>.Ignored, A<string>.Ignored, A<string>.Ignored))
                    .ReturnsLazily(() => ValueTask.CompletedTask);
            A.CallTo(() => otherSubscriber.EarseAsync(A<ConsumerContext>.Ignored, A<int>.Ignored))
                    .ReturnsLazily(() => ValueTask.CompletedTask);

            #endregion // A.CallTo(...).ReturnsLazily(...)

            await SendSequenceAsync(producer);

            CancellationToken cancellation = GetCancellationToken();
            using var firstAttemptCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            var consumerPipe = _consumerBuilder
                             .WithOptions(o => DefaultOptions(o, 3, AckBehavior.OnSucceed))
                             .WithCancellation(cancellation)
                             .Environment(ENV)
                             .Uri(URI)
                             .WithResiliencePolicy(Policy.Handle<Exception>().RetryAsync(3))
                             .WithLogger(_fakeLogger);

            await using IConsumerLifetime otherSubscription = consumerPipe
                             .WithCancellation(firstAttemptCancellation.Token)
                             .Group(CONSUMER_GROUP)
                             .Name($"TEST FAULTED {DateTime.UtcNow:HH:mm:ss}")
                             .Subscribe(new SequenceOperationsConsumerBridge(otherSubscriber));

            await otherSubscription.Completion;

            await using IConsumerLifetime subscription = consumerPipe
                            .WithOptions(o => o with
                            {
                                ClaimingTrigger = o.ClaimingTrigger with
                                {
                                    MinIdleTime = TimeSpan.FromMilliseconds(50)
                                }
                            })
                             .Group(CONSUMER_GROUP)
                             .Name($"TEST OK {DateTime.UtcNow:HH:mm:ss}")
                             .Subscribe(new SequenceOperationsConsumerBridge(_subscriber1));

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await subscription.Completion;

            #region Validation

            A.CallTo(() => otherSubscriber.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                        .MustHaveHappened(
                                    (3 /* Polly retry */),
                                    Times.OrMore);
            A.CallTo(() => _subscriber1.RegisterAsync(A<ConsumerContext>.Ignored, A<User>.Ignored))
                        .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber1.LoginAsync(A<ConsumerContext>.Ignored, "admin", "1234"))
                        .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber1.EarseAsync(A<ConsumerContext>.Ignored, 4335))
                        .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // Claim_Test

        #region SendSequenceAsync

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
    }
}
