using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

using EventSourcing.Backbone.Building;
using EventSourcing.Backbone.Channels.RedisProvider;
using EventSourcing.Backbone.Enums;
using EventSourcing.Backbone.UnitTests.Entities;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Polly;

using StackExchange.Redis;

using Xunit;
using Xunit.Abstractions;

using static EventSourcing.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;

#pragma warning disable S3881 // "IDisposable" should be implemented correctly

// docker run -p 6379:6379 -it --rm --name redis-event-source redislabs/rejson:latest

namespace EventSourcing.Backbone.Tests
{
    /// <summary>
    /// The end to end tests.
    /// </summary>
    public class AckPatternsTests : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly ISequenceOperationsConsumer _subscriber = A.Fake<ISequenceOperationsConsumer>();
        private readonly SequenceOperationsConsumerBridge _subscriberBridge;
        private readonly IProducerStoreStrategyBuilder _producerBuilder;
        private readonly IConsumerStoreStrategyBuilder _consumerBuilder;

        private readonly string ENV = $"test";
        private readonly string URI = $"{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}:{Guid.NewGuid():N}";

        private readonly ILogger _fakeLogger = A.Fake<ILogger>();
        private static readonly User USER = new User { Eracure = new Personal { Name = "mike", GovernmentId = "A25" }, Comment = "Do it" };
        private const int TIMEOUT = 1_000 * 50;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AckPatternsTests"/> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
        /// <param name="producerChannelBuilder">The producer channel builder.</param>
        /// <param name="consumerChannelBuilder">The consumer channel builder.</param>
        public AckPatternsTests(
            ITestOutputHelper outputHelper,
            Func<IProducerStoreStrategyBuilder, ILogger, IProducerStoreStrategyBuilder>? producerChannelBuilder = null,
             Func<IConsumerStoreStrategyBuilder, ILogger, IConsumerStoreStrategyBuilder>? consumerChannelBuilder = null)
        {
            _outputHelper = outputHelper;
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

            A.CallTo(() => _subscriber.RegisterAsync(A<User>.Ignored))
                    .ReturnsLazily(() =>
                    {
                        Metadata meta = ConsumerMetadata.Context;
                        if (string.IsNullOrEmpty(meta.EventKey))
                            return ValueTask.FromException(new EventSourcingException("Event Key is missing"));
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

        #region OnSucceed_ACK_WithFailure_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task OnSucceed_ACK_WithFailure_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            .Environment(ENV)
                                            //.WithOptions(producerOption)
                                            .Uri(URI)
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

            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, 4, AckBehavior.OnSucceed))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithResiliencePolicy(Policy.Handle<Exception>().RetryAsync(3, (ex, i) => _outputHelper.WriteLine($"Retry {i}")))
                         .WithLogger(_fakeLogger)
                         .Group("CONSUMER_GROUP_1")
                         .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                         .Subscribe(_subscriberBridge);

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

        [Fact(Timeout = TIMEOUT)]
        public async Task OnFinaly_ACK_WithFailure_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            .Environment(ENV)
                                            .Uri(URI)
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


            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                         .WithOptions(o => DefaultOptions(o, 3, AckBehavior.OnFinally))
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithResiliencePolicy(Policy.Handle<Exception>().RetryAsync(3, (ex, i) => _outputHelper.WriteLine($"Retry {i}")))
                         .WithLogger(_fakeLogger)
                         .Group("CONSUMER_GROUP_1")
                         .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                         .Subscribe(_subscriberBridge);

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

        [Fact(Timeout = TIMEOUT)]
        public async Task Manual_ACK_Test()
        {
            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            .Environment(ENV)
                                            //.WithOptions(producerOption)
                                            .Uri(URI)
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
                    ConsumerMetadata meta = ConsumerMetadata.Context;
                    await meta.AckAsync();
                    //await Ack.Current.AckAsync();
                });
            A.CallTo(() => _subscriber.EarseAsync(A<int>.Ignored))
                    .ReturnsLazily(() => Ack.Current.AckAsync());

            #endregion // A.CallTo(...).ReturnsLazily(...)

            await SendSequenceAsync(producer);

            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _consumerBuilder
                                .WithOptions(o => DefaultOptions(o, 4, AckBehavior.Manual))
                                .WithCancellation(cancellation)
                                .Environment(ENV)
                                .Uri(URI)
                                .WithResiliencePolicy(Policy.Handle<Exception>().RetryAsync(3, (ex, i) => _outputHelper.WriteLine($"Retry {i}")))
                                .WithLogger(_fakeLogger)
                                .Group("CONSUMER_GROUP_1")
                                .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                                .Subscribe(_subscriberBridge);

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

        #region SendSequenceAsync

        private static async Task<EventKeys> SendSequenceAsync(ISequenceOperationsProducer producer, string pass = "1234")
        {
            EventKey r1 = await producer.RegisterAsync(USER with { Comment = null });
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


        ~AckPatternsTests()
        {
            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            try
            {
                IConnectionMultiplexer conn = RedisClientFactory.CreateProviderAsync(
                                                    logger: _fakeLogger,
                                                    configurationHook: cfg => cfg.AllowAdmin = true).Result;
                string serverName = Environment.GetEnvironmentVariable(END_POINT_KEY) ?? "localhost:6379";
                var server = conn.GetServer(serverName);
                IEnumerable<RedisKey> keys = server.Keys(pattern: $"*{URI}*");
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
