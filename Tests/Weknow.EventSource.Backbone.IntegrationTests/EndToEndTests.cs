using FakeItEasy;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;


// TODO: [bnaya 2020-10] delete REDIS key on dispose
// TODO: [bnaya 2020-10] ensure message order(cancel ack should cancel all following messages)
// TODO: [bnaya 2020-10] check for no pending

namespace Weknow.EventSource.Backbone.Tests
{
    public class EndToEndTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly ISequenceOperations _subscriber = A.Fake<ISequenceOperations>();
        private readonly CancellationToken _testScopeCancellation = GetCancellationToken();
        private readonly IProducerOptionsBuilder _producerBuilder;
        private readonly IConsumerOptionsBuilder _condumerBuilder;

        #region Ctor

        public EndToEndTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _producerBuilder = ProducerBuilder.Empty.UseRedisProducerChannel(
                                        _testScopeCancellation,
                                        configuration: (cfg) => cfg.ServiceName = "mymaster");
            _condumerBuilder = ConsumerBuilder.Empty.UseRedisConsumerChannel(
                                        _testScopeCancellation,
                                        configuration: (cfg) => cfg.ServiceName = "mymaster");
        }

        #endregion // Ctor

        #region OnSucceed_ACK_Test

        [Fact]
        public async Task OnSucceed_ACK_Test()
        {
            string partition = $"test-{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}-{Guid.NewGuid():N}";
            string shard = $"some-shard-{DateTime.UtcNow.Second}";

            #region ISequenceOperations producer = ...

            ISequenceOperations producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Partition(partition)
                                            .Shard(shard)
                                            .Build<ISequenceOperations>();

            #endregion // ISequenceOperations producer = ...

            await SendSequenceAsync(producer);

            var consumerOptions = new ConsumerOptions(
                                        AckBehavior.OnSucceed,
                                        maxMessages: 3 /* detach consumer after 3 messages*/);
            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _condumerBuilder
                         .WithOptions(consumerOptions)
                         .WithCancellation(cancellation)
                         .Partition(partition)
                         .Shard(shard)
                         .Subscribe(meta => _subscriber, "CONSUMER_GROUP_1", $"TEST {DateTime.UtcNow:HH:mm:ss}");

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


        #region OnSucceed_ACK_WithFailure_Test

        [Fact]
        public async Task OnSucceed_ACK_WithFailure_Test()
        {
            string partition = $"test-{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}-{Guid.NewGuid():N}";
            string shard = $"some-shard-{DateTime.UtcNow.Second}";

            #region ISequenceOperations producer = ...

            ISequenceOperations producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Partition(partition)
                                            .Shard(shard)
                                            .Build<ISequenceOperations>();

            #endregion // ISequenceOperations producer = ...

            int tryNumber = 0;
            A.CallTo(() => _subscriber.LoginAsync(A<string>.Ignored, A<string>.Ignored))
             .Throws<Exception>(e => Interlocked.Increment(ref tryNumber) == 1 ? e : null);

            await SendSequenceAsync(producer);

            var consumerOptions = new ConsumerOptions(
                                        AckBehavior.OnSucceed,
                                        maxMessages: 4 /* detach consumer after 4 messages*/);
            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _condumerBuilder
                         .WithOptions(consumerOptions)
                         .WithCancellation(cancellation)
                         .Partition(partition)
                         .Shard(shard)
                         .Subscribe(meta => _subscriber, "CONSUMER_GROUP_1", $"TEST {DateTime.UtcNow:HH:mm:ss}");

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

        #endregion // OnSucceed_ACK_WithFailure_Test

        // TODO: [bnaya 2020-10] manual ack
        #region Manual_ACK_Test

        [Fact]
        public async Task Manual_ACK_Test()
        {
            string partition = $"test-{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}-{Guid.NewGuid():N}";
            string shard = $"some-shard-{DateTime.UtcNow.Second}";

            #region ISequenceOperations producer = ...

            ISequenceOperations producer = _producerBuilder
                                            //.WithOptions(producerOption)
                                            .Partition(partition)
                                            .Shard(shard)
                                            .Build<ISequenceOperations>();

            #endregion // ISequenceOperations producer = ...

            int tryNumber = 0;
            A.CallTo(() => _subscriber.LoginAsync(A<string>.Ignored, A<string>.Ignored))
             .Throws<Exception>(e => Interlocked.Increment(ref tryNumber) == 1 ? e : null);


            await SendSequenceAsync(producer);

            var consumerOptions = new ConsumerOptions(
                                        AckBehavior.Manual, 
                                        maxMessages: 4 /* detach consumer after 4 messages*/);
            CancellationToken cancellation = GetCancellationToken();

            #region await using IConsumerLifetime subscription = ...Subscribe(...)

            await using IConsumerLifetime subscription = _condumerBuilder
                         .WithOptions(consumerOptions)
                         .WithCancellation(cancellation)
                         .Partition(partition)
                         .Shard(shard)
                         .Subscribe(meta => _subscriber, "CONSUMER_GROUP_1", $"TEST {DateTime.UtcNow:HH:mm:ss}");

            #endregion // await using IConsumerLifetime subscription = ...Subscribe(...)

            await subscription.Completion;

            #region Validation

            A.CallTo(() => _subscriber.RegisterAsync(A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.LoginAsync("admin", "1234"))
                .MustHaveHappened(2, Times.Exactly);
            A.CallTo(() => _subscriber.EarseAsync(4335))
                .MustHaveHappenedOnceExactly();

            #endregion // Validation
        }

        #endregion // Manual_ACK_Test

        #region SendSequenceAsync

        /// <summary>
        /// Sends standard test sequence.
        /// </summary>
        /// <param name="producer">The producer.</param>
        private static async Task SendSequenceAsync(ISequenceOperations producer)
        {
            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);
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
