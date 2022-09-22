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
    public class MigrationReceiverTest // : IDisposable
    {
        private const string TARGET_KEY = "REDIS_ANALYSTS_ENDPOINT";
        private const string SOURCE_KEY = "REDIS_EVENT_SOURCE_ENDPOINT";

        private readonly ITestOutputHelper _outputHelper;
        private readonly ISequenceOperationsConsumer _subscriber = A.Fake<ISequenceOperationsConsumer>();
        private readonly SequenceOperationsConsumerBridge _subscriberBridge;
        private readonly IProducerStoreStrategyBuilder _targetProducerBuilder;
        private readonly IConsumerStoreStrategyBuilder _sourceConsumerBuilder;
        //private readonly string ENV = "Production";
        private readonly string ENV = "Development";
        //private readonly string PARTITION = "analysts";
        private readonly string PARTITION = $"{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}:{Guid.NewGuid():N}";
        private readonly string SHARD = "default";

        private readonly ILogger _fakeLogger = A.Fake<ILogger>();
        private const int TIMEOUT = 1_000 * 30;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationReceiverTest"/> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
        /// <param name="producerChannelBuilder">The producer channel builder.</param>
        /// <param name="consumerChannelBuilder">The consumer channel builder.</param>
        public MigrationReceiverTest(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _targetProducerBuilder = ProducerBuilder.Empty.UseRedisChannel(credentialsKeys: new RedisCredentialsKeys { EndpointKey = TARGET_KEY })
                .AddVoidStrategy();
            //.AddS3Strategy();

            _sourceConsumerBuilder = ConsumerBuilder.Empty.UseRedisChannel(credentialsKeys: new RedisCredentialsKeys { EndpointKey = SOURCE_KEY })
                                        .AddS3Strategy(new S3Options { Bucket = "event-source-storage", EnvironmentConvension = S3EnvironmentConvention.BucketPrefix });
        }

        #endregion // Ctor


        #region Migration_By_Receiver_Test

        [Fact(Timeout = TIMEOUT, Skip = "Use to migrate data between 2 different souces")]
        public async Task Migration_By_Receiver_Test()
        {
            #region IRawProducer rawProducer = ...

            IRawProducer rawProducer = _targetProducerBuilder
                                            .Environment(ENV)
                                            .Partition(PARTITION)
                                            .Shard(SHARD)
                                            .WithLogger(_fakeLogger)
                                            .BuildRaw(new RawProducerOptions { KeepOriginalMeta = true });

            #endregion // IRawProducer rawProducer = ...


            CancellationToken cancellation = GetCancellationToken();

            IAsyncEnumerable<Announcement> announcements = _sourceConsumerBuilder
                         .WithCancellation(cancellation)
                         .Environment(ENV)
                         .Partition(PARTITION)
                         .Shard(SHARD)
                         .WithLogger(_fakeLogger)
                         .BuildIterator()
                         .GetAsyncEnumerable(new ConsumerAsyncEnumerableOptions { ExitWhenEmpty = true });
            await foreach (var announcement in announcements.WithCancellation(cancellation))
            {
                _fakeLogger.LogInformation(announcement.ToString());
                await rawProducer.Produce(announcement);
            }


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
                                : TimeSpan.FromSeconds(10)).Token;
        }

        #endregion // GetCancellationToken

        #region // Dispose pattern


        //~MigrationReceiverTest()
        //{
        //    Dispose();
        //}

        //public void Dispose()
        //{
        //    GC.SuppressFinalize(this);
        //    try
        //    {
        //        IConnectionMultiplexer conn = RedisClientFactory.CreateProviderBlocking(
        //                                            cfg => cfg.AllowAdmin = true);
        //        string serverName = Environment.GetEnvironmentVariable(END_POINT_KEY) ?? "localhost:6379";
        //        var server = conn.GetServer(serverName);
        //        IEnumerable<RedisKey> keys = server.Keys(pattern: $"*{PARTITION}*");
        //        IDatabaseAsync db = conn.GetDatabase();

        //        var ab = new ActionBlock<string>(k => LocalAsync(k), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 30 });
        //        foreach (string key in keys)
        //        {
        //            ab.Post(key);
        //        }

        //        ab.Complete();
        //        ab.Completion.Wait();

        //        async Task LocalAsync(string k)
        //        {
        //            try
        //            {
        //                await db.KeyDeleteAsync(k, CommandFlags.DemandMaster);
        //                _outputHelper.WriteLine($"Cleanup: delete key [{k}]");
        //            }
        //            #region Exception Handling

        //            catch (RedisTimeoutException ex)
        //            {
        //                _outputHelper.WriteLine($"Test dispose timeout error (delete keys) {ex.FormatLazy()}");
        //            }
        //            catch (Exception ex)
        //            {
        //                _outputHelper.WriteLine($"Test dispose timeout error (delete keys) {ex.FormatLazy()}");
        //            }

        //            #endregion // Exception Handling
        //        }
        //    }
        //    #region Exception Handling

        //    catch (RedisTimeoutException ex)
        //    {
        //        _outputHelper.WriteLine($"Test dispose timeout error (delete keys) {ex.FormatLazy()}");
        //    }
        //    catch (Exception ex)
        //    {
        //        _outputHelper.WriteLine($"Test dispose error (delete keys) {ex.FormatLazy()}");
        //    }

        //    #endregion // Exception Handling
        //}

        #endregion // Dispose pattern

        #region SubscriptionBridge

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

        #endregion // SubscriptionBridge
    }
}
