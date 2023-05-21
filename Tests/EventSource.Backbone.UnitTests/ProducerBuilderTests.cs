using System.Threading.Channels;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using EventSource.Backbone.Building;
using EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;



namespace EventSource.Backbone
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducerBuilderTests"/> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
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
                        .Uri("Kids:HappySocks")
                        .AddInterceptor(_rawAsyncInterceptor);

            var producerB =
                _builder.UseTestProducerChannel1()
                        .Uri("NGOs:NGO #2782228")
                        .UseSegmentation(_segmentationStrategy);

            var producerC =
                _builder.UseTestProducerChannel2()
                        .Uri("Fans:Geek: @someone")
                        .UseSegmentation(_otherSegmentationStrategy);


            ISequenceOfProducer producer =
                                    _builder
                                        .Merge(producerA, producerB, producerC)
                                        .UseSegmentation(_postSegmentationStrategy)
                                        .CustomBuildSequenceOfProducer();

            string[] ids1 = await producer.RegisterAsync(new User());
            string[] ids2 = await producer.LoginAsync("admin", "1234");
            string[] ids3 = await producer.EarseAsync(4335);

            Assert.True(ids1.SequenceEqual(new[] { "1", "A", "#a" }));
            Assert.True(ids2.SequenceEqual(new[] { "2", "B", "#b" }));
            Assert.True(ids3.SequenceEqual(new[] { "3", "C", "#c" }));
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
                        .Uri("Kids:HappySocks")
                        .BuildSequenceOperationsProducer();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);

            var message = await ch.Reader.ReadAsync();
        }

        #endregion // Build_Serializer_Producer_Test

        #region Build_GeneratedFactory_Producer_Test

        [Fact]
        public async Task Build_GeneratedFactory_Producer_Test()
        {
            //var option = new EventSourceOptions(_serializer);

            ISequenceOfProducer producer =
                _builder.UseChannel(_channel)
                        //.WithOptions(option)
                        .Uri("Kids:HappySocks")
                        .BuildSequenceOfProducer();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);

            var message = await ch.Reader.ReadAsync();
        }

        #endregion // Build_GeneratedFactory_Producer_Test

        #region Build_GeneratedFactory_Override_Producer_Test

        [Fact]
        public async Task Build_GeneratedFactory_Specialize_Producer_Test()
        {
            //var option = new EventSourceOptions(_serializer);

            ISequenceOfProducer producer =
                _builder.UseChannel(_channel)
                        //.WithOptions(option)
                        .Uri("Kids:HappySocks")
                        .Specialize<ISequenceOfProducer>()
                        .Environment("QA")
                        .BuildSequenceOfProducer();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);

            var message = await ch.Reader.ReadAsync();
        }

        #endregion // Build_GeneratedFactory_Specialize_Producer_Test

        #region Build_Factory_Producer_Test

        [Fact]
        public async Task Build_Factory_Producer_Test()
        {
            //var option = new EventSourceOptions(_serializer);

            ISequenceOperationsProducer producer =
                _builder.UseChannel(_channel)
                        //.WithOptions(option)
                        .Uri("Kids:HappySocks")
                        .BuildSequenceOperationsProducer();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);

            var message = await ch.Reader.ReadAsync();
        }

        #endregion // Build_Factory_Producer_Test

        #region Build_Factory_Producer_WithReturn_Test

        [Fact]
        public async Task Build_Factory_Producer_WithReturn_Test()
        {
            //var option = new EventSourceOptions(_serializer);

            ISequenceOfProducer producer =
                _builder.UseChannel(_channel)
                        //.WithOptions(option)
                        .Uri("Kids:HappySocks")
                        .CustomBuildSequenceOfProducer();

            string id1 = await producer.RegisterAsync(new User());
            string id2 = await producer.LoginAsync("admin", "1234");
            string id3 = await producer.EarseAsync(4335);

            Assert.Equal("1", id1);
            Assert.Equal("2", id2);
            Assert.Equal("3", id3);
            var message = await ch.Reader.ReadAsync();
        }

        #endregion // Build_Factory_Producer_WithReturn_Test

        #region Build_Factory_Override_Producer_Test

        [Fact]
        public async Task Build_Factory_Specialize_Producer_Test()
        {
            //var option = new EventSourceOptions(_serializer);

            ISequenceOperationsProducer producer =
                _builder.UseChannel(_channel)
                        //.WithOptions(option)
                        .Uri("Kids:HappySocks")
                        .Specialize<ISequenceOperationsProducer>()
                        .Environment("QA")
                        .BuildSequenceOperationsProducer();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);

            var message = await ch.Reader.ReadAsync();
        }

        #endregion // Build_Factory_Specialize_Producer_Test

        #region Build_Interceptor_Producer_Test

        [Fact]
        public async Task Build_Interceptor_Producer_Test()
        {
            ISequenceOperationsProducer producer =
                _builder.UseChannel(_channel)
                        .Uri("Kids:HappySocks")
                        .AddInterceptor(_rawInterceptor)
                        .AddInterceptor(_rawAsyncInterceptor)
                        .UseSegmentation(_segmentationStrategy)
                        .UseSegmentation(_otherSegmentationStrategy)
                        .BuildSequenceOperationsProducer();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);
        }

        #endregion // Build_Interceptor_Producer_Test
    }

}
