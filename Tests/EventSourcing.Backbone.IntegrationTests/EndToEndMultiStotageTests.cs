using System.Diagnostics;
using System.Text.Json;

using EventSourcing.Backbone.Building;
using EventSourcing.Backbone.Tests.Entities;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using Xunit;
using Xunit.Abstractions;

#pragma warning disable S3881 // "IDisposable" should be implemented correctly

// docker run -p 6379:6379 -it --rm --name redis-evt-src redislabs/rejson:latest


// TODO: [bnaya 2020-10] ensure message order(cancel ack should cancel all following messages)
// TODO: [bnaya 2020-10] check for no pending

namespace EventSourcing.Backbone.Tests
{
    /// <summary>
    /// The end to end explicit tests.
    /// </summary>
    public class EndToEndMultiStotageTests : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IEventFlowConsumer _subscriber = A.Fake<IEventFlowConsumer>();

        private readonly string URI = $"{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}:{Guid.NewGuid():N}";

        private readonly ILogger _fakeLogger = A.Fake<ILogger>();

        private static readonly Person _person = new Person(10, "Tom");
        private static readonly JsonElement _personElement = JsonDocument.Parse(File.ReadAllText("person.json")).RootElement;
        private static readonly JsonElement _payloadElement = JsonDocument.Parse(File.ReadAllText("payload.json")).RootElement;
        private readonly string ENV = $"test";
        private const int TIMEOUT = 1000 * 20;

        private static readonly S3ConsumerOptions OPTIONS = new S3ConsumerOptions
        {
            EnvironmentConvention = S3EnvironmentConvention.BucketPrefix,
            BasePath = "tests"
        };

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="EndToEndMultiStotageTests"/> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
        public EndToEndMultiStotageTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            A.CallTo(() => _subscriber.Stage1Async(A<ConsumerContext>.Ignored, A<Person>.Ignored, A<string>.Ignored))
                    .ReturnsLazily(() => ValueTask.CompletedTask);
            A.CallTo(() => _subscriber.Stage2Async(A<ConsumerContext>.Ignored, A<JsonElement>.Ignored, A<JsonElement>.Ignored))
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

        #region StorageSplitting_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task StorageSplitting_Test()
        {
            var sw = Stopwatch.StartNew();

            #region IEventFlow producer = ...

            StorageBehavior behavior1 = new StorageBehavior
            {
                Filter = (meta, k) => k == "PII"
            };

            StorageBehavior behavior2 = new StorageBehavior
            {
                Filter = (meta, k) => k != "PII"
            };

            IEventFlowProducer producer = ProducerBuilder.Empty.UseRedisChannel()
                                .AddS3Storage(OPTIONS, behavior1)
                                .AddRedisHashStorage(behavior2)
                                .Environment(ENV)
                                //.WithOptions(producerOption)
                                .Uri(URI)
                                //.WithLogger(_fakeLogger)
                                .BuildEventFlowProducer();

            #endregion // IEventFlow producer = ...

            var snapshot = sw.Elapsed;
            _outputHelper.WriteLine($"Build producer = {snapshot:mm\\:ss\\.ff}");
            snapshot = sw.Elapsed;

            await SendSequenceAsync(producer);

            snapshot = sw.Elapsed - snapshot;
            _outputHelper.WriteLine($"Produce = {snapshot:mm\\:ss\\.ff}");
            snapshot = sw.Elapsed;

            var consumerOptions = new ConsumerOptions
            {
                AckBehavior = AckBehavior.OnSucceed,
                MaxMessages = 2 /* detach consumer after 2 messages*/
            };
            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            IConsumerSubscribeBuilder builder = ConsumerBuilder.Empty.UseRedisChannel()
                         .AddRedisHashStorage()
                         .AddS3Storage(OPTIONS)
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

            snapshot = sw.Elapsed - snapshot;
            _outputHelper.WriteLine($"Build Consumer = {snapshot:mm\\:ss\\.ff}");
            snapshot = sw.Elapsed;

            await subscription.Completion;

            snapshot = sw.Elapsed - snapshot;
            _outputHelper.WriteLine($"Consumed = {snapshot:mm\\:ss\\.ff}");

            #region Validation

            A.CallTo(() => _subscriber.Stage1Async(A<ConsumerContext>.Ignored, A<Person>.Ignored, A<string>.Ignored))
                        .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.Stage2Async(A<ConsumerContext>.Ignored, A<JsonElement>.Ignored, A<JsonElement>.Ignored))
                        .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // StorageSplitting_Test

        #region StorageCache_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task StorageCache_Test()
        {
            IConsumerStorageStrategy fakeStorageFirst = A.Fake<IConsumerStorageStrategy>();
            A.CallTo(() => fakeStorageFirst.LoadBucketAsync(
                                    A<Metadata>.Ignored,
                                    A<Bucket>.Ignored,
                                    A<EventBucketCategories>.Ignored,
                                    A<Func<string, string>>.Ignored,
                                    A<CancellationToken>.Ignored))
                                .Returns(Bucket.Empty.ToValueTask());
            A.CallTo(() => fakeStorageFirst.Name)
                                .Returns("Fake 1");
            IConsumerStorageStrategy fakeStorageLast = A.Fake<IConsumerStorageStrategy>();
            A.CallTo(() => fakeStorageLast.LoadBucketAsync(
                                    A<Metadata>.Ignored,
                                    A<Bucket>.Ignored,
                                    A<EventBucketCategories>.Ignored,
                                    A<Func<string, string>>.Ignored,
                                    A<CancellationToken>.Ignored))
                                .ReturnsLazily<ValueTask<Bucket>, Metadata, Bucket, EventBucketCategories, Func<string, string>, CancellationToken>(
                                    (meta, prv, cat, getPrp, cancellationToken) => prv.ToValueTask());
            A.CallTo(() => fakeStorageLast.Name)
                                .Returns("Fake 2");

            var sw = Stopwatch.StartNew();

            #region IEventFlow producer = ...

            StorageBehavior behavior = TimeSpan.FromMicroseconds(1); // time to live
            IEventFlowProducer producer = ProducerBuilder.Empty.UseRedisChannel()
                                .AddRedisHashStorage(behavior)
                                .AddS3Storage(OPTIONS)
                                .Environment(ENV)
                                //.WithOptions(producerOption)
                                .Uri(URI)
                                //.WithLogger(_fakeLogger)
                                .BuildEventFlowProducer();

            #endregion // IEventFlow producer = ...

            var snapshot = sw.Elapsed;
            _outputHelper.WriteLine($"Build producer = {snapshot:mm\\:ss\\.ff}");
            snapshot = sw.Elapsed;

            await SendSequenceAsync(producer);

            await Task.Delay(40);
            snapshot = sw.Elapsed - snapshot;
            _outputHelper.WriteLine($"Produce = {snapshot:mm\\:ss\\.ff}");
            snapshot = sw.Elapsed;

            var consumerOptions = new ConsumerOptions
            {
                AckBehavior = AckBehavior.OnSucceed,
                MaxMessages = 2 /* detach consumer after 2 messages*/
            };
            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            IConsumerSubscribeBuilder builder = ConsumerBuilder.Empty.UseRedisChannel()
                         .AddStorageStrategyFactory(logger => fakeStorageFirst)
                         .AddRedisHashStorage()
                         .AddS3Storage(OPTIONS)
                         .AddStorageStrategyFactory(logger => fakeStorageLast)
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

            snapshot = sw.Elapsed - snapshot;
            _outputHelper.WriteLine($"Build Consumer = {snapshot:mm\\:ss\\.ff}");
            snapshot = sw.Elapsed;

            await subscription.Completion;

            snapshot = sw.Elapsed - snapshot;
            _outputHelper.WriteLine($"Consumed = {snapshot:mm\\:ss\\.ff}");

            #region Validation

            A.CallTo(() => _subscriber.Stage1Async(A<ConsumerContext>.Ignored, A<Person>.Ignored, A<string>.Ignored))
                        .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.Stage2Async(A<ConsumerContext>.Ignored, A<JsonElement>.Ignored, A<JsonElement>.Ignored))
                        .MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeStorageFirst.LoadBucketAsync(
                                    A<Metadata>.Ignored,
                                    A<Bucket>.Ignored,
                                    EventBucketCategories.Segments,
                                    A<Func<string, string>>.Ignored,
                                    A<CancellationToken>.Ignored))
                        .MustHaveHappenedTwiceExactly(); // 2 operations 
            A.CallTo(() => fakeStorageLast.LoadBucketAsync(
                                    A<Metadata>.Ignored,
                                    A<Bucket>.Ignored,
                                    EventBucketCategories.Segments,
                                    A<Func<string, string>>.Ignored,
                                    A<CancellationToken>.Ignored))
                        .MustHaveHappenedTwiceExactly(); // 2 operations 

            #endregion // Validation
        }

        #endregion // StorageCache_Test

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

        ~EndToEndMultiStotageTests()
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
