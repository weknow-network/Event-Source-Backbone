using FakeItEasy;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;



namespace Weknow.EventSource.Backbone.Tests
{
    public class EndToEndTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly ISequenceOperations _subscriber = A.Fake<ISequenceOperations>();

        #region Ctor

        public EndToEndTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        #endregion // Ctor

        #region Build_Serializer_Producer_Test

        [Fact]
        public async Task Build_Serializer_Producer_Test()
        {
            string partition = $"test-{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}-{Guid.NewGuid():N}";
            string shard = $"some-shard-{DateTime.UtcNow.Second}";

            ISequenceOperations producer =
                ProducerBuilder.Empty.UseRedisProducerChannel(
                                        CancellationToken.None,
                                        configuration: (cfg) => cfg.ServiceName = "mymaster")
                        //.WithOptions(producerOption)
                        .Partition(partition)
                        .Shard(shard)
                        .Build<ISequenceOperations>();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);

            var consumerOptions = new EventSourceConsumerOptions(
                                                AckBehavior.OnSucceed,
                                                maxMessages: 3);

            var cts = new CancellationTokenSource(Debugger.IsAttached ? TimeSpan.FromMinutes(10) : TimeSpan.FromSeconds(10));
            await using IConsumerLifetime subscription =
                 ConsumerBuilder.Empty.UseRedisConsumerChannel(
                                        CancellationToken.None,
                                        configuration: (cfg) => cfg.ServiceName = "mymaster")
                         .WithOptions(consumerOptions)
                         .WithCancellation(cts.Token)
                         .Partition(partition)
                         .Shard(shard)
                         .Subscribe(meta => _subscriber, "CONSUMER_GROUP_1", $"TEST {DateTime.UtcNow:HH:mm:ss}");

            await subscription.Completion;

            A.CallTo(() => _subscriber.RegisterAsync(A<User>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.LoginAsync("admin", "1234"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.EarseAsync(4335))
                .MustHaveHappenedOnceExactly();
        }

        #endregion // Build_Serializer_Producer_Test
    }
}
