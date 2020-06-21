using FakeItEasy;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;

namespace Weknow.EventSource.Backbone
{
    public class EventSourceConsumerApiDesignTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IEventSourceConsumerBuilder _builder = A.Fake<IEventSourceConsumerBuilder>();
        private readonly IDataSerializer _serializer = A.Fake<IDataSerializer>();
        private readonly IConsumerRawInterceptor _rawInterceptor = A.Fake<IConsumerRawInterceptor>();
        private readonly IConsumerRawAsyncInterceptor _rawAsyncInterceptor = A.Fake<IConsumerRawAsyncInterceptor>();
        private readonly IConsumerInterceptor<User> _interceptor = A.Fake<IConsumerInterceptor<User>>();
        private readonly IConsumerAsyncInterceptor<User> _asyncInterceptor = A.Fake<IConsumerAsyncInterceptor<User>>();
        private readonly IConsumerSegmenationProvider<User> _segmentor = A.Fake<IConsumerSegmenationProvider<User>>();

        public EventSourceConsumerApiDesignTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        #region Build_Raw_Consumer_Test

        [Fact]
        public void Build_Raw_Consumer_Test()
        {
            ISourceBlock<Ackable<AnnouncementRaw>> consumer =
                _builder.BuildRaw();
        }

        #endregion // Build_Raw_Consumer_Test        

        #region Build_Serializer_Consumer_Test

        [Fact]
        public void Build_Serializer_Consumer_Test()
        {
            var option = new EventSourceOptions(_serializer);
            ISourceBlock<Ackable<AnnouncementRaw>> consumer =
                            _builder
                                .WithOptions(option)
                                .BuildRaw();
        }

        #endregion // Build_Serializer_Consumer_Test        

        #region Build_Raw_Interceptor_Consumer_Test

        [Fact]
        public void Build_Raw_Interceptor_Consumer_Test()
        {
            ISourceBlock<Ackable<AnnouncementRaw>> consumer =
                _builder
                    .AddInterceptor(_rawInterceptor)
                    .BuildRaw();
        }

        #endregion // Build_Raw_Interceptor_Consumer_Test        

        #region Build_Raw_AsyncInterceptor_Consumer_Test

        [Fact]
        public void Build_Raw_AsyncInterceptor_Consumer_Test()
        {
            ISourceBlock<Ackable<AnnouncementRaw>> consumer =
                _builder
                    .AddAsyncInterceptor(_rawAsyncInterceptor)
                    .BuildRaw();
        }

        #endregion // Build_Raw_AsyncInterceptor_Consumer_Test        

        #region Build_Typed_Consumer_Test

        [Fact]
        public void Build_Typed_Consumer_Test()
        {
            ISourceBlock<Ackable<Announcement<User>>> consumer =
                _builder
                    .ForType<User>(_segmentor, "ADD_USER")
                    .Build();
        }

        #endregion // Build_Typed_Consumer_Test        

        #region Build_Typed_Lambda_Segmentation_Consumer_Test

        [Fact]
        public void Build_Typed_Lambda_Segmentation_Consumer_Test()
        {
            ISourceBlock<Ackable<Announcement<User>>> consumer =
                _builder
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
                _builder
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
                _builder
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
                _builder
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
                _builder
                    .ForType<User>(_segmentor, "ADD_USER")
                    .AddAsyncInterceptor(_asyncInterceptor)
                    .Build();
        }

        #endregion // Build_Typed_AsyncInterceptor_Consumer_Test        
    }
}
