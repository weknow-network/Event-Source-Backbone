using System.Diagnostics;

using EventSourcing.Backbone.Building;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

// docker run -p 6379:6379 -it --rm --name redis-event-source redislabs/rejson:latest

namespace EventSourcing.Backbone.Tests
{
    /// <summary>
    /// The end to end tests.
    /// </summary>
    public class MigrationReceiverTest // : IDisposable
    {
        private const string TARGET_KEY = "REDIS_MIGRATION_TARGET_ENDPOINT";
        private const string SOURCE_KEY = "REDIS_EVENT_SOURCE_ENDPOINT";

        private readonly ITestOutputHelper _outputHelper;
        private readonly IProducerStoreStrategyBuilder _targetProducerBuilder;
        private readonly IConsumerStoreStrategyBuilder _sourceConsumerBuilder;
        private readonly string ENV = "Production";
        private readonly string URI = "analysts";
        private readonly ILogger _fakeLogger = A.Fake<ILogger>();
        private const int TIMEOUT = 1_000 * 300;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationReceiverTest" /> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
        public MigrationReceiverTest(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _targetProducerBuilder = ProducerBuilder.Empty.UseRedisChannel(credentialsKeys: new RedisCredentialsKeys { EndpointKey = TARGET_KEY })
                .AddVoidStrategy();

            _sourceConsumerBuilder = ConsumerBuilder.Empty.UseRedisChannel(credentialsKeys: new RedisCredentialsKeys { EndpointKey = SOURCE_KEY })
                                        .AddS3Strategy(new S3Options { Bucket = "event-source-storage", EnvironmentConvension = S3EnvironmentConvention.BucketPrefix });
        }

        #endregion // Ctor


        #region Migration_By_Receiver_Test

        [Fact(Timeout = TIMEOUT, Skip = "Use to migrate data between 2 different sources")]
        //[Fact(Timeout = TIMEOUT)]
        public async Task Migration_By_Receiver_Test()
        {
            #region IRawProducer rawProducer = ...

            IRawProducer rawProducer = _targetProducerBuilder
                                            .Environment(ENV)
                                            .Uri(URI)
                                            .WithLogger(_fakeLogger)
                                            .BuildRaw(new RawProducerOptions { KeepOriginalMeta = true });

            #endregion // IRawProducer rawProducer = ...


            CancellationToken cancellation = GetCancellationToken();

            IAsyncEnumerable<Announcement> announcements = _sourceConsumerBuilder
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Uri(URI)
                         .WithLogger(_fakeLogger)
                         .BuildIterator()
                         .GetAsyncEnumerable(new ConsumerAsyncEnumerableOptions { ExitWhenEmpty = true });
            int count = 0;
            await foreach (var announcement in announcements.WithCancellation(cancellation))
            {
                _fakeLogger.LogInformation(announcement.ToString());
                await rawProducer.Produce(announcement);
                count++;
                _outputHelper.WriteLine($"{count} events processed");
            }
            _outputHelper.WriteLine($"Total events = {count}");
        }

        #endregion // Migration_By_Receiver_Test


        #region GetCancellationToken

        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        /// <returns></returns>
        private static CancellationToken GetCancellationToken()
        {
            return new CancellationTokenSource(Debugger.IsAttached
                                ? TimeSpan.FromMinutes(10)
                                : TimeSpan.FromMinutes(5)).Token;
        }

        #endregion // GetCancellationToken
    }
}
