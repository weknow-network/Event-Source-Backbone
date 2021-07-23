using FakeItEasy;

using Microsoft.Extensions.Logging;

using System;
using System.Threading.Channels;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;



namespace Weknow.EventSource.Backbone
{
    public class ProducerBuilderTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IProducerBuilder _builder = ProducerBuilder.Empty;
        private readonly Func<ILogger, IProducerChannelProvider> _channel;
        //private readonly IDataSerializer _serializer;
        private readonly IProducerInterceptor _rawInterceptor = A.Fake<IProducerInterceptor>();
        private readonly IProducerAsyncInterceptor _rawAsyncInterceptor = A.Fake<IProducerAsyncInterceptor>();
        private readonly IProducerAsyncSegmentationStrategy _segmentationStrategy = A.Fake<IProducerAsyncSegmentationStrategy>();
        private readonly IProducerSegmentationStrategy _otherSegmentationStrategy = A.Fake<IProducerSegmentationStrategy>();
        private readonly IProducerSegmentationStrategy _postSegmentationStrategy = A.Fake<IProducerSegmentationStrategy>();
        private readonly Channel<Announcement> ch;

        #region Ctor

        public ProducerBuilderTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            //_serializer = new JsonDataSerializer();
            ch = Channel.CreateUnbounded<Announcement>();
            _channel = _ => new ProducerTestChannel(ch);
        }

        #endregion // Ctor

        #region Build_Merge_Producer_Test

        [Fact]
        public async Task Build_Merge_Producer_Test()
        {
            var producerA =
                _builder.UseChannel(_channel)
                        .Partition("Organizations")
                        .Shard("Org: #RedSocks")
                        .AddInterceptor(_rawAsyncInterceptor);

            var producerB =
                _builder.UseTestProducerChannel()
                        .Partition("NGOs")
                        .Shard("NGO #2782228")
                        .UseSegmentation(_segmentationStrategy);

            var producerC =
                _builder.UseChannel(_channel)
                        .Partition("Fans")
                        .Shard("Geek: @someone")
                        .UseSegmentation(_otherSegmentationStrategy);


            ISequenceOperationsProducer producer =
                                    _builder
                                        .Merge(producerA, producerB, producerC)
                                        .UseSegmentation(_postSegmentationStrategy)
                                        .Build<ISequenceOperationsProducer>();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);
        }

        #endregion // Build_Merge_Producer_Test

        #region Build_Serializer_Producer_Test

        [Fact]
        public async Task Build_Serializer_Producer_Test()
        {
            //var option = new EventSourceOptions(_serializer);

            ISequenceOperationsProducer producer =
                _builder.UseChannel(_channel)
                        //.WithOptions(option)
                        .Partition("Organizations")
                        .Shard("Org: #RedSocks")
                        .Build<ISequenceOperationsProducer>();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);

            var message = await ch.Reader.ReadAsync();
        }

        #endregion // Build_Serializer_Producer_Test

        #region Build_Interceptor_Producer_Test

        [Fact]
        public async Task Build_Interceptor_Producer_Test()
        {
            ISequenceOperationsProducer producer =
                _builder.UseChannel(_channel)
                        .Partition("Organizations")
                        .Shard("Org: #RedSocks")
                        .AddInterceptor(_rawInterceptor)
                        .AddInterceptor(_rawAsyncInterceptor)
                        .UseSegmentation(_segmentationStrategy)
                        .UseSegmentation(_otherSegmentationStrategy)
                        .Build<ISequenceOperationsProducer>();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);
        }

        #endregion // Build_Interceptor_Producer_Test
    }
}
