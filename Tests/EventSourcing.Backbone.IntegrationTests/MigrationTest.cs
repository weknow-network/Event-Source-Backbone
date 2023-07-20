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
    public class MigrationTest : TestsBase
    {
        private readonly ISequenceOperationsConsumer _subscriber = A.Fake<ISequenceOperationsConsumer>();
        private readonly SequenceOperationsConsumerBridge _subscriberBridge;
        private readonly IProducerStoreStrategyBuilder _producerBuilder;
        private readonly IConsumerStoreStrategyBuilder _consumerBuilder;
        private readonly string ENV = $"Development";
        protected override string URI { get; } = $"{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}:{Guid.NewGuid():N}";

        private static readonly User USER = new User { Eracure = new Personal { Name = "mike", GovernmentId = "A25" }, Comment = "Do it" };

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationTest" /> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
        public MigrationTest(
            ITestOutputHelper outputHelper) : base(outputHelper)
        {
            _producerBuilder = ProducerBuilder.Empty.UseRedisChannel( /*,
                                        configuration: (cfg) => cfg.ServiceName = "mymaster" */);
            var stg = new RedisConsumerChannelSetting
            {
                DelayWhenEmptyBehavior = new DelayWhenEmptyBehavior
                {
                    CalcNextDelay = ((d, _) => TimeSpan.FromMilliseconds(2))
                }
            };
            _consumerBuilder = stg.CreateRedisConsumerBuilder();

            A.CallTo(() => _subscriber.RegisterAsync(A<ConsumerMetadata>.Ignored, A<User>.Ignored))
                    .ReturnsLazily(() =>
                    {
                        Metadata meta = ConsumerMetadata.Context;
                        if (string.IsNullOrEmpty(meta.EventKey))
                            return ValueTask.FromException(new EventSourcingException("Event Key is missing"));
                        return ValueTask.CompletedTask;
                    });
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

            _subscriberBridge = new SequenceOperationsConsumerBridge(_subscriber);
        }

        #endregion // Ctor

        #region ConsumerOptions DefaultOptions

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

        #endregion // ConsumerOptions DefaultOptions

        #region Migration_Test

        [Fact(Timeout = TIMEOUT)]
        public async Task Migration_Test()
        {
            string MIGRATE_TO = "Migrated";

            #region ISequenceOperations producer = ...

            ISequenceOperationsProducer producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Environment(ENV)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildSequenceOperationsProducer();

            #endregion // ISequenceOperations producer = ...

            await SendSequenceAsync(producer);

            ISequenceOperationsConsumer emptySubscriber = A.Fake<ISequenceOperationsConsumer>();

            CancellationToken cancellation = GetCancellationToken();
            var cts = new CancellationTokenSource();
            try
            {
                #region var consumerBuilder = _consumerBuilder...

                var consumerBuilder = _consumerBuilder
                             .WithOptions(o => DefaultOptions(o, 3, AckBehavior.OnSucceed))
                             .WithCancellation(cancellation)
                             .Uri(URI)
                             .WithLogger(_fakeLogger);

                #endregion // var consumerBuilder = _consumerBuilder...

                #region  await using IConsumerLifetime subscriptionMigration = ...Subscribe(...)

                // listen on the migrated data
                await using IConsumerLifetime subscriptionMigration = consumerBuilder
                             .WithOptions(o => o with { OriginFilter = MessageOrigin.Copy })
                             .Environment(MIGRATE_TO)
                             .Group("CONSUMER_GROUP_2")
                             .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                             .Subscribe(_subscriberBridge);

                // listen on the migrated data for original messages (shouldn't handle any message)
                SequenceOperationsConsumerBridge subscriberBridgeNone = new SequenceOperationsConsumerBridge(emptySubscriber);
                await using IConsumerLifetime subscriptionMigrationFilterAll = consumerBuilder
                             .WithOptions(o => o with { OriginFilter = MessageOrigin.Original })
                             .Environment(MIGRATE_TO)
                             .Group("CONSUMER_GROUP_3")
                             .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                             .WithCancellation(cts.Token)
                             .Subscribe(subscriberBridgeNone);

                #endregion //  await using IConsumerLifetime subscriptionMigration = ...Subscribe(...)

                #region await using IConsumerLifetime fwSubscription = ...forwarder

                // will forward messages (produce at the announcement level)
                IRawProducer fwProducer = _producerBuilder
                                                .Environment(MIGRATE_TO)
                                                .BuildRaw();

                // attach the producer into a subscription bridge
                ISubscriptionBridge fwSubscriberBridge = fwProducer.ToSubscriptionBridge();

                // attach the forward subscription into a concrete stream
                await using IConsumerLifetime fwSubscription = consumerBuilder
                             .Environment(ENV)
                             .Group("CONSUMER_GROUP_1")
                             .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                             .Subscribe(fwSubscriberBridge);

                #endregion await using IConsumerLifetime fwSubscription = ...forwarder

                await fwSubscription.Completion;
                await subscriptionMigration.Completion;
            }
            finally
            {
                cts.CancelSafe();
            }

            #region Validation

            A.CallTo(() => _subscriber.RegisterAsync(A<ConsumerMetadata>.Ignored, A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.LoginAsync(A<ConsumerMetadata>.Ignored, "admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.EarseAsync(A<ConsumerMetadata>.Ignored, 4335))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => emptySubscriber.RegisterAsync(A<ConsumerMetadata>.Ignored, A<User>.Ignored))
                .MustNotHaveHappened();

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
    }
}
