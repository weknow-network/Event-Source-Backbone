using EventSourcing.Backbone.UnitTests.Entities;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace EventSourcing.Backbone
{
    public class ConsumerBuilderTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IConsumerBuilder _builder = ConsumerBuilder.Empty;
        private readonly Func<ILogger, IConsumerChannelProvider> _channel = A.Fake<Func<ILogger, IConsumerChannelProvider>>();
        private readonly IConsumerAsyncSegmentationStrategy _segmentation = A.Fake<IConsumerAsyncSegmentationStrategy>();
        private readonly IConsumerInterceptor _rawInterceptor = A.Fake<IConsumerInterceptor>();
        private readonly IConsumerAsyncInterceptor _rawAsyncInterceptor = A.Fake<IConsumerAsyncInterceptor>();
        //private readonly EventSourceConsumerOptions _options;
        private readonly ISequenceOfConsumer _subscriber = A.Fake<ISequenceOfConsumer>();

        public ConsumerBuilderTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            //var serializer = new JsonDataSerializer();
            //_options = new EventSourceConsumerOptions(serializer: serializer);
        }

        #region Build_Raw_Consumer_Direct_Test

        [Fact]
        public void Build_Raw_Consumer_Direct_Test()
        {
            IAsyncDisposable disposePrtition =
                 _builder
                         .UseChannel(_channel)
                         //.WithOptions(_options)
                         .RegisterInterceptor(_rawAsyncInterceptor)
                         .RegisterInterceptor(_rawInterceptor)
                         .RegisterSegmentationStrategy(_segmentation)
                         .Uri("ORDERS")
                         // .Shard("ORDER-AHS7821X")
                         .SubscribeSequenceOfConsumer(_subscriber);

        }

        #endregion // Build_Raw_Consumer_Direct_Test        
    }
}
