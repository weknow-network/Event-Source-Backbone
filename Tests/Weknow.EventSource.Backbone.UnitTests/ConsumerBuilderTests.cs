using FakeItEasy;

using Microsoft.Extensions.Logging;

using System;

using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;

namespace Weknow.EventSource.Backbone
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
        private readonly ISequenceOperationsConsumer _subscriber = A.Fake<ISequenceOperationsConsumer>();

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
                         .Partition("ORDERS")
                         // .Shard("ORDER-AHS7821X")
                         .SubscribeDeprecated(_subscriber);

        }

        #endregion // Build_Raw_Consumer_Direct_Test        
    }
}
