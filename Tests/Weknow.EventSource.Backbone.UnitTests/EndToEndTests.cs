using FakeItEasy;

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;



namespace Weknow.EventSource.Backbone
{
    public class EndToEndTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IProducerBuilder _producerBuilder = ProducerBuilder.Empty;
        private readonly IConsumerBuilder _consumerBuilder = ConsumerBuilder.Empty;
        private readonly IProducerChannelProvider _producerChannel;
        private readonly IConsumerChannelProvider _consumerChannel;
        // private readonly IDataSerializer _serializer;
        private readonly IProducerInterceptor _rawInterceptor = A.Fake<IProducerInterceptor>();
        private readonly IProducerAsyncInterceptor _rawAsyncInterceptor = A.Fake<IProducerAsyncInterceptor>();
        private readonly IProducerAsyncSegmentationStrategy _segmentationStrategy = A.Fake<IProducerAsyncSegmentationStrategy>();
        private readonly IProducerSegmentationStrategy _otherSegmentationStrategy = A.Fake<IProducerSegmentationStrategy>();
        private readonly IProducerSegmentationStrategy _postSegmentationStrategy = A.Fake<IProducerSegmentationStrategy>();
        private readonly Channel<Announcement> ch;
        private readonly ISequenceOperationsConsumer _subscriber = A.Fake<ISequenceOperationsConsumer>();

        #region Ctor

        public EndToEndTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            // _serializer = new JsonDataSerializer();
            ch = Channel.CreateUnbounded<Announcement>();
            _producerChannel = new ProducerTestChannel(ch);
            _consumerChannel = new ConsumerTestChannel(ch);
        }

        #endregion // Ctor

        #region Build_Serializer_Producer_Test

        [Fact]
        public async Task Build_Serializer_Producer_Test()
        {
            //var producerOption = new EventSourceOptions(_serializer);

            ISequenceOperationsProducer producer =
                _producerBuilder.UseChannel(_producerChannel)
                        //.WithOptions(producerOption)
                        .Partition("Organizations")
                        .Shard("Org: #RedSocks")
                        .Build<ISequenceOperationsProducer>();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);

            //var consumerOptions = new EventSourceConsumerOptions(serializer: _serializer);

            var cts = new CancellationTokenSource();
            IAsyncDisposable subscription =
                 _consumerBuilder.UseChannel(_consumerChannel)
                         //.WithOptions(consumerOptions)
                         .WithCancellation(cts.Token)
                         .Partition("Organizations")
                         .Shard("Org: #RedSocks")
                         .Subscribe(meta => _subscriber);

            ch.Writer.Complete();
            await subscription.DisposeAsync();
            await ch.Reader.Completion;

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
