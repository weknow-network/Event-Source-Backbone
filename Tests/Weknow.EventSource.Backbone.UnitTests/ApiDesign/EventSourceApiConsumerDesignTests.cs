using FakeItEasy;

using System.Threading.Tasks.Dataflow;

using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;

namespace Weknow.EventSource.Backbone
{
    public class EventSourceConsumerApiDesignTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IEventSourceConsumerChannelBuilder _builder = A.Fake<IEventSourceConsumerChannelBuilder>();
        private readonly IConsumerChannelProvider _channel = A.Fake<IConsumerChannelProvider>();
        private readonly IDataSerializer _serializer = A.Fake<IDataSerializer>();
        private readonly IConsumerRawInterceptor _rawInterceptor = A.Fake<IConsumerRawInterceptor>();
        private readonly IConsumerRawAsyncInterceptor _rawAsyncInterceptor = A.Fake<IConsumerRawAsyncInterceptor>();
        private readonly IConsumerInterceptor<User> _interceptor = A.Fake<IConsumerInterceptor<User>>();
        private readonly IConsumerAsyncInterceptor<User> _asyncInterceptor = A.Fake<IConsumerAsyncInterceptor<User>>();
        private readonly IConsumerSegmenationStrategy<User> _segmentor = A.Fake<IConsumerSegmenationStrategy<User>>();

        public EventSourceConsumerApiDesignTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        #region Build_Raw_Consumer_Direct_Test

        [Fact]
        public void Build_Raw_Consumer_Direct_Test()
        {
            ISourceBlock<Ackable<AnnouncementRaw>> consumer =
                _builder.UseChannel(_channel)
                        .Shard("ORDER-AHS7821X")
                        .BuildRaw();
            ISourceBlock<Ackable<AnnouncementRaw>> consumer =
                _builder.UseChannel(_channel)
                        .Group("ORDERS")
                        .BuildRaw();
        }

        #endregion // Build_Raw_Consumer_Direct_Test        

        #region Build_Raw_Consumer_ByTags_Test

        [Fact]
        public void Build_Raw_Consumer_ByTags_Test()
        {
            ISourceBlock<Ackable<AnnouncementRaw>> consumer =
                _builder.UseChannel(_channel)
                        //-----.RegisterTags("ORDERS", "SHIPMENTS")
                        .BuildRaw();
        }

        #endregion // Build_Raw_Consumer_ByTags_Test        

        #region Build_Raw_Consumer_WithTestChannel_Test

        [Fact]
        public void Build_Raw_Consumer_WithTestChannel_Test()
        {
            ISourceBlock<Ackable<AnnouncementRaw>> consumer =
                _builder.UseTestConsumerChannel()
                        .FromShard("ORDER-AHS7821X")
                        .BuildRaw();
        }

        #endregion // Build_Raw_Consumer_WithTestChannel_Test        

        #region Build_Serializer_Consumer_Test

        [Fact]
        public void Build_Serializer_Consumer_Test()
        {
            var option = new EventSourceOptions(_serializer);
            ISourceBlock<Ackable<AnnouncementRaw>> consumer =
                            _builder.UseChannel(_channel)
                                    .WithOptions(option)
                                    .FromShard("ORDER-AHS7821X")
                                    .BuildRaw();
        }

        #endregion // Build_Serializer_Consumer_Test        

        #region Build_Raw_Interceptor_Consumer_Test

        [Fact]
        public void Build_Raw_Interceptor_Consumer_Test()
        {
            ISourceBlock<Ackable<AnnouncementRaw>> consumer =
                _builder.UseChannel(_channel)
                        .RegisterInterceptor(_rawInterceptor)
                        .BuildRaw();
        }

        #endregion // Build_Raw_Interceptor_Consumer_Test        

        #region Build_Raw_AsyncInterceptor_Consumer_Test

        [Fact]
        public void Build_Raw_AsyncInterceptor_Consumer_Test()
        {
            ISourceBlock<Ackable<AnnouncementRaw>> consumer =
                _builder.UseChannel(_channel)
                        .RegisterAsyncInterceptor(_rawAsyncInterceptor)
                        .BuildRaw();
        }

        #endregion // Build_Raw_AsyncInterceptor_Consumer_Test        

        #region Build_Typed_Consumer_Test

        [Fact]
        public void Build_Typed_Consumer_Test()
        {
            ISourceBlock<Ackable<Announcement<User>>> consumer =
                _builder.UseChannel(_channel)
                        .ForType<User>(_segmentor, "ADD_USER")
                        .Build();
        }

        #endregion // Build_Typed_Consumer_Test        

        #region Build_Typed_Lambda_Segmentation_Consumer_Test

        [Fact]
        public void Build_Typed_Lambda_Segmentation_Consumer_Test()
        {
            ISourceBlock<Ackable<Announcement<User>>> consumer =
                _builder.UseChannel(_channel)
                        .ForType<User>((segments, serializer) => 
                                    new User
                                    {
                                        Eracure = serializer.Deserialize<User.Personal>(segments["Eracure"]),
                                        Details = serializer.Deserialize<User.Anonymous>(segments["Open"])
                                    }, "ADD_USER")
                        .Build();
        }

        #endregion // Build_Typed_Lambda_Segmentation_Consumer_Test        

        #region Build_Typed_Filter_Consumer_Test

        [Fact]
        public void Build_Typed_Filter_Consumer_Test()
        {
            ISourceBlock<Ackable<Announcement<User>>> consumer =
                _builder.UseChannel(_channel)
                        .ForType<User>(
                                _segmentor, 
                                meta => meta.DataType == nameof(User))
                        .Build();
        }

        #endregion // Build_Typed_CBuild_Typed_Filter_Consumer_Testonsumer_Test        

        #region Build_Typed_Lambda_Segmentation_Filter_Consumer_Test

        [Fact]
        public void Build_Typed_Lambda_Segmentation_Filter_Consumer_Test()
        {
            ISourceBlock<Ackable<Announcement<User>>> consumer =
                _builder.UseChannel(_channel)
                        .ForType<User>((segments, serializer) => 
                                    new User
                                    {
                                        Eracure = serializer.Deserialize<User.Personal>(segments["Eracure"]),
                                        Details = serializer.Deserialize<User.Anonymous>(segments["Open"])
                                    },
                                    meta => meta.DataType == nameof(User))
                        .Build();
        }

        #endregion // Build_Typed_Lambda_Segmentation_Filter_Consumer_Test        

        #region Build_Typed_Interceptor_Consumer_Test

        [Fact]
        public void Build_Typed_Interceptor_Consumer_Test()
        {
            ISourceBlock<Ackable<Announcement<User>>> consumer =
                _builder.UseChannel(_channel)
                        .ForType<User>(_segmentor, "ADD_USER")
                        .AddInterceptor(_interceptor)
                        .Build();
        }

        #endregion // Build_Typed_Interceptor_Consumer_Test        

        #region Build_Typed_AsyncInterceptor_Consumer_Test

        [Fact]
        public void Build_Typed_AsyncInterceptor_Consumer_Test()
        {
            ISourceBlock<Ackable<Announcement<User>>> consumer =
                _builder.UseChannel(_channel)
                        .ForType<User>(_segmentor, "ADD_USER")
                        .AddAsyncInterceptor(_asyncInterceptor)
                        .Build();
        }

        #endregion // Build_Typed_AsyncInterceptor_Consumer_Test        
    }
}
