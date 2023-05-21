using FakeItEasy;

using Microsoft.Extensions.Logging;

using EventSourcing.Backbone.Building;
using EventSourcing.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;



namespace EventSourcing.Backbone
{
    public class EventSourceApiProducerDesignTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IProducerBuilder _builder = A.Fake<IProducerBuilder>();
        private readonly ILogger _logger = A.Fake<ILogger>();
        private readonly Func<ILogger, IProducerChannelProvider> _channel = A.Fake<Func<ILogger, IProducerChannelProvider>>();
        private readonly IDataSerializer _serializer = A.Fake<IDataSerializer>();
        private readonly IProducerInterceptor _rawInterceptor = A.Fake<IProducerInterceptor>();
        private readonly IProducerAsyncInterceptor _rawAsyncInterceptor = A.Fake<IProducerAsyncInterceptor>();
        private readonly IProducerAsyncSegmentationStrategy _segmentationStrategy = A.Fake<IProducerAsyncSegmentationStrategy>();
        private readonly IProducerSegmentationStrategy _otherSegmentationStrategy = A.Fake<IProducerSegmentationStrategy>();
        private readonly IProducerSegmentationStrategy _postSegmentationStrategy = A.Fake<IProducerSegmentationStrategy>();

        #region Ctor

        public EventSourceApiProducerDesignTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        #endregion // Ctor

        #region Build_API_Merge_Producer_Test

        [Fact]
        public async Task Build_API_Merge_Producer_Test()
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
                _builder.UseChannel(_channel)
                        .Uri("Fans:Geek: @someone")
                        .UseSegmentation(_otherSegmentationStrategy);


            ISequenceOperationsProducer producer =
                                    _builder
                                        .Merge(producerA, producerB, producerC)
                                        .UseSegmentation(_postSegmentationStrategy)
                                        .WithLogger(_logger)
                                        .BuildCustomSequenceOperationsProducer();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);
        }

        #endregion // Build_Default_Producer_Test

        #region Build_API_Serializer_Producer_Test

        [Fact]
        public async Task Build_API_Serializer_Producer_Test()
        {
            var option = new EventSourceOptions { Serializer = _serializer };

            ISequenceOperationsProducer producer =
                _builder.UseChannel(_channel)
                        .WithOptions(option)
                        .Uri("Kids:HappySocks")
                        .WithLogger(_logger)
                        .BuildSequenceOperationsProducer();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);
        }

        #endregion // Build_API_Serializer_Producer_Test

        #region Build_API_Interceptor_Producer_Test

        [Fact]
        public async Task Build_API_Interceptor_Producer_Test()
        {
            ISequenceOperationsProducer producer =
                _builder.UseChannel(_channel)
                        .Uri("Kids:HappySocks")
                        .AddInterceptor(_rawInterceptor)
                        .AddInterceptor(_rawAsyncInterceptor)
                        .UseSegmentation(_segmentationStrategy)
                        .UseSegmentation(_otherSegmentationStrategy)
                        .WithLogger(_logger)
                        .BuildSequenceOperationsProducer();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);
        }

        #endregion // Build_API_Interceptor_Producer_Test
    }
}
