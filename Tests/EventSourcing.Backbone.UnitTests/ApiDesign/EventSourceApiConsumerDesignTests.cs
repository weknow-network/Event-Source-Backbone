using EventSourcing.Backbone.UnitTests.Entities;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace EventSourcing.Backbone
{
    public class EventSourceConsumerApiDesignTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly ILogger _logger = A.Fake<ILogger>();
        private readonly IConsumerBuilder _builder = A.Fake<IConsumerBuilder>();
        private readonly Func<ILogger, IConsumerChannelProvider> _channel = A.Fake<Func<ILogger, IConsumerChannelProvider>>();
        private readonly IConsumerAsyncSegmentationStrategy _segmentation = A.Fake<IConsumerAsyncSegmentationStrategy>();
        private readonly IConsumerInterceptor _rawInterceptor = A.Fake<IConsumerInterceptor>();
        private readonly IConsumerAsyncInterceptor _rawAsyncInterceptor = A.Fake<IConsumerAsyncInterceptor>();
        private readonly ConsumerOptions _options = new ConsumerOptions();
        private readonly ISequenceOfConsumer _subscriber = A.Fake<ISequenceOfConsumer>();

        public EventSourceConsumerApiDesignTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        #region Build_Raw_Consumer_Direct_Test

        [Fact]
        public void Build_Raw_Consumer_Direct_Test()
        {
            IAsyncDisposable disposePrtition =
                 _builder.UseChannel(_channel)
                         .WithOptions(o => _options)
                         .RegisterInterceptor(_rawAsyncInterceptor)
                         .RegisterInterceptor(_rawInterceptor)
                         .RegisterSegmentationStrategy(_segmentation)
                         .Uri("ORDERS")
                         .WithLogger(_logger)
                         .SubscribeSequenceOfConsumer(_subscriber);

        }

        #endregion // Build_Raw_Consumer_Direct_Test                
    }
}
