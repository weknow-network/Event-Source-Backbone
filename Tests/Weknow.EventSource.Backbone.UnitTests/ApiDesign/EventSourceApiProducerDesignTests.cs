using FakeItEasy;

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;

namespace Weknow.EventSource.Backbone
{
    public class EventSourceApiProducerDesignTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IEventSourceProducerChannelBuilder _builder = A.Fake<IEventSourceProducerChannelBuilder>();
        private readonly IProducerChannelProvider _channel = A.Fake<IProducerChannelProvider>();
        private readonly IDataSerializer _serializer = A.Fake<IDataSerializer>();
        private readonly IProducerRawInterceptor _rawInterceptor = A.Fake<IProducerRawInterceptor>();
        private readonly IProducerRawAsyncInterceptor _rawAsyncInterceptor = A.Fake<IProducerRawAsyncInterceptor>();
        private readonly IProducerInterceptor<User> _interceptor = A.Fake<IProducerInterceptor<User>>();
        private readonly IProducerAsyncInterceptor<User> _asyncInterceptor = A.Fake<IProducerAsyncInterceptor<User>>();
        private readonly IProducerSegmenationProvider<User> _segmentor = A.Fake<IProducerSegmenationProvider<User>>();

        #region Ctor

        public EventSourceApiProducerDesignTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        #endregion // Ctor

        #region Build_Default_Producer_Test

        [Fact]
        public async Task Build_Default_Producer_Test()
        {
            IEventSourceProducer<User> producer =
                _builder.UseChannel(_channel)
                        .ForEventType<User>("ADD_USER")
                        .Build();

            await producer.SendAsync(new User());
            await producer.SendAsync(new User(), "UPD_USER");
        }

        #endregion // Build_Default_Producer_Test

        #region Build_Raw_Producer_WithTestChannel_Test

        [Fact]
        public void Build_Raw_Producer_WithTestChannel_Test()
        {
            IEventSourceProducer<User> producer =
                _builder.UseTestProducerChannel()
                        .ForEventType<User>("ADD_USER")
                        .Build();
        }

        #endregion // Build_Raw_Producer_WithTestChannel_Test        

        #region Build_Serializer_Producer_Test

        [Fact]
        public async Task Build_Serializer_Producer_Test()
        {
            var option = new EventSourceOptions(_serializer);
            IEventSourceProducer<User> producer =
                _builder.UseChannel(_channel)
                        .WithOptions(option)
                        .ForEventType<User>("ADD_USER")
                        .Build();

            await producer.SendAsync(new User());
        }

        #endregion // Build_Serializer_Producer_Test

        #region Build_Interceptor_Producer_Test

        [Fact]
        public async Task Build_Interceptor_Producer_Test()
        {
            IEventSourceProducer<User> producer =
                _builder.UseChannel(_channel)
                        .AddInterceptor(_rawInterceptor)
                        .ForEventType<User>("ADD_USER")
                        .Build();

            await producer.SendAsync(new User());
        }

        #endregion // Build_Interceptor_Producer_Test

        #region Build_AsyncInterceptor_Producer_Test

        [Fact]
        public async Task Build_AsyncInterceptor_Producer_Test()
        {
            IEventSourceProducer<User> producer =
                _builder.UseChannel(_channel)
                        .AddAsyncInterceptor(_rawAsyncInterceptor)
                        .ForEventType<User>("ADD_USER")
                        .Build();

            await producer.SendAsync(new User());
        }

        #endregion // Build_AsyncInterceptor_Producer_Test

        #region Build_TypedInterceptor_Producer_Test

        [Fact]
        public async Task Build_TypedInterceptor_Producer_Test()
        {
            IEventSourceProducer<User> producer =
                _builder.UseChannel(_channel)
                        .ForEventType<User>("ADD_USER")
                        .AddInterceptor(_interceptor)
                        .Build();

            await producer.SendAsync(new User());
        }

        #endregion // Build_TypedInterceptor_Producer_Test

        #region Build_TypedAsyncInterceptor_Producer_Test

        [Fact]
        public async Task Build_TypedAsyncInterceptor_Producer_Test()
        {
            IEventSourceProducer<User> producer =
                _builder.UseChannel(_channel)
                        .ForEventType<User>("ADD_USER")
                        .AddAsyncInterceptor(_asyncInterceptor)
                        .Build();

            await producer.SendAsync(new User());
        }

        #endregion // Build_TypedAsyncInterceptor_Producer_Test

        #region Build_Segmentation_Producer_Test

        [Fact]
        public async Task Build_Segmentation_Producer_Test()
        {
            IEventSourceProducer<User> producer =
                _builder.UseChannel(_channel)
                        .ForEventType<User>("ADD_USER")
                        .AddSegmentationProvider(_segmentor)
                        .Build();

            await producer.SendAsync(new User());
        }

        #endregion // Build_Segmentation_Producer_Test

        #region Build_LambdaSegmentation_Producer_Test

        [Fact]
        public async Task Build_LambdaSegmentation_Producer_Test()
        {
            IEventSourceProducer<User> producer =
                _builder.UseChannel(_channel)
                        .ForEventType<User>("ADD_USER")
                        .AddSegmentationProvider((user, serializer) =>
                                    ImmutableDictionary<string, ReadOnlyMemory<byte>>
                                            .Empty
                                            .Add("Personal", serializer.Serialize(user.Eracure))
                                            .Add("Open", serializer.Serialize(user.Details)))
                        .Build();

            await producer.SendAsync(new User());
        }

        #endregion // Build_LambdaSegmentation_Producer_Test
    }
}
