using FakeItEasy;

using System;
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
        // private readonly IDataSerializer _serializer;
        //private readonly IProducerInterceptor _rawInterceptor = A.Fake<IProducerInterceptor>();
        //private readonly IProducerAsyncInterceptor _rawAsyncInterceptor = A.Fake<IProducerAsyncInterceptor>();
        //private readonly IProducerAsyncSegmentationStrategy _segmentationStrategy = A.Fake<IProducerAsyncSegmentationStrategy>();
        //private readonly IProducerSegmentationStrategy _otherSegmentationStrategy = A.Fake<IProducerSegmentationStrategy>();
        //private readonly IProducerSegmentationStrategy _postSegmentationStrategy = A.Fake<IProducerSegmentationStrategy>();
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

            ISequenceOperations producer =
                ProducerBuilder.Empty.UseRedisProducerChannel(CancellationToken.None)
                        //.WithOptions(producerOption)
                        .Partition(partition)
                        .Shard("Org: #RedSocks")
                        .Build<ISequenceOperations>();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);

            var consumerOptions = new EventSourceConsumerOptions(
                                                AckBehavior.OnSucceed,
                                                maxMessages: 3);

            var cts = new CancellationTokenSource();
            await using IConsumerLifetime subscription =
                 ConsumerBuilder.Empty.UseRedisConsumerChannel(CancellationToken.None)
                         .WithOptions(consumerOptions)
                         .WithCancellation(cts.Token)
                         .Partition(partition)
                         .Shard("Org: #RedSocks")
                         .Subscribe(meta => _subscriber, "CONSUMER_GROUP_1");

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
