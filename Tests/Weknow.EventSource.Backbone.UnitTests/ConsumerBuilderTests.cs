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
                        .Subscribe(meta => _subscriber);

        }

        #endregion // Build_Raw_Consumer_Direct_Test        

        //#region Build_Raw_Consumer_ByTags_Test

        //[Fact]
        //public void Build_Raw_Consumer_ByTags_Test()
        //{
        //    ISourceBlock<Ackable<AnnouncementRaw>> consumer =
        //        _builder.UseChannel(_channel)
        //                //-----.RegisterTags("ORDERS", "SHIPMENTS")
        //                .BuildRaw();
        //}

        //#endregion // Build_Raw_Consumer_ByTags_Test        

        //#region Build_Raw_Consumer_WithTestChannel_Test

        //[Fact]
        //public void Build_Raw_Consumer_WithTestChannel_Test()
        //{
        //    ISourceBlock<Ackable<AnnouncementRaw>> consumer =
        //        _builder.UseTestConsumerChannel()
        //                .FromShard("ORDER-AHS7821X")
        //                .BuildRaw();
        //}

        //#endregion // Build_Raw_Consumer_WithTestChannel_Test        

        //#region Build_Serializer_Consumer_Test

        //[Fact]
        //public void Build_Serializer_Consumer_Test()
        //{
        //    var option = new EventSourceOptions(_serializer);
        //    ISourceBlock<Ackable<AnnouncementRaw>> consumer =
        //                    _builder.UseChannel(_channel)
        //                            .WithOptions(option)
        //                            .FromShard("ORDER-AHS7821X")
        //                            .BuildRaw();
        //}

        //#endregion // Build_Serializer_Consumer_Test        

        //#region Build_Raw_Interceptor_Consumer_Test

        //[Fact]
        //public void Build_Raw_Interceptor_Consumer_Test()
        //{
        //    ISourceBlock<Ackable<AnnouncementRaw>> consumer =
        //        _builder.UseChannel(_channel)
        //                .RegisterInterceptor(_rawInterceptor)
        //                .BuildRaw();
        //}

        //#endregion // Build_Raw_Interceptor_Consumer_Test        

        //#region Build_Raw_AsyncInterceptor_Consumer_Test

        //[Fact]
        //public void Build_Raw_AsyncInterceptor_Consumer_Test()
        //{
        //    ISourceBlock<Ackable<AnnouncementRaw>> consumer =
        //        _builder.UseChannel(_channel)
        //                .RegisterAsyncInterceptor(_rawAsyncInterceptor)
        //                .BuildRaw();
        //}

        //#endregion // Build_Raw_AsyncInterceptor_Consumer_Test        

        //#region Build_Typed_Consumer_Test

        //[Fact]
        //public void Build_Typed_Consumer_Test()
        //{
        //    ISourceBlock<Ackable<Announcement<User>>> consumer =
        //        _builder.UseChannel(_channel)
        //                .ForType<User>(_segmentor, "ADD_USER")
        //                .Build();
        //}

        //#endregion // Build_Typed_Consumer_Test        

        //#region Build_Typed_Lambda_Segmentation_Consumer_Test

        //[Fact]
        //public void Build_Typed_Lambda_Segmentation_Consumer_Test()
        //{
        //    ISourceBlock<Ackable<Announcement<User>>> consumer =
        //        _builder.UseChannel(_channel)
        //                .ForType<User>((segments, serializer) => 
        //                            new User
        //                            {
        //                                Eracure = serializer.Deserialize<User.Personal>(segments["Eracure"]),
        //                                Details = serializer.Deserialize<User.Anonymous>(segments["Open"])
        //                            }, "ADD_USER")
        //                .Build();
        //}

        //#endregion // Build_Typed_Lambda_Segmentation_Consumer_Test        

        //#region Build_Typed_Filter_Consumer_Test

        //[Fact]
        //public void Build_Typed_Filter_Consumer_Test()
        //{
        //    ISourceBlock<Ackable<Announcement<User>>> consumer =
        //        _builder.UseChannel(_channel)
        //                .ForType<User>(
        //                        _segmentor, 
        //                        meta => meta.DataType == nameof(User))
        //                .Build();
        //}

        //#endregion // Build_Typed_CBuild_Typed_Filter_Consumer_Testonsumer_Test        

        //#region Build_Typed_Lambda_Segmentation_Filter_Consumer_Test

        //[Fact]
        //public void Build_Typed_Lambda_Segmentation_Filter_Consumer_Test()
        //{
        //    ISourceBlock<Ackable<Announcement<User>>> consumer =
        //        _builder.UseChannel(_channel)
        //                .ForType<User>((segments, serializer) => 
        //                            new User
        //                            {
        //                                Eracure = serializer.Deserialize<User.Personal>(segments["Eracure"]),
        //                                Details = serializer.Deserialize<User.Anonymous>(segments["Open"])
        //                            },
        //                            meta => meta.DataType == nameof(User))
        //                .Build();
        //}

        //#endregion // Build_Typed_Lambda_Segmentation_Filter_Consumer_Test        

        //#region Build_Typed_Interceptor_Consumer_Test

        //[Fact]
        //public void Build_Typed_Interceptor_Consumer_Test()
        //{
        //    ISourceBlock<Ackable<Announcement<User>>> consumer =
        //        _builder.UseChannel(_channel)
        //                .ForType<User>(_segmentor, "ADD_USER")
        //                .AddInterceptor(_interceptor)
        //                .Build();
        //}

        //#endregion // Build_Typed_Interceptor_Consumer_Test        

        //#region Build_Typed_AsyncInterceptor_Consumer_Test

        //[Fact]
        //public void Build_Typed_AsyncInterceptor_Consumer_Test()
        //{
        //    ISourceBlock<Ackable<Announcement<User>>> consumer =
        //        _builder.UseChannel(_channel)
        //                .ForType<User>(_segmentor, "ADD_USER")
        //                .AddAsyncInterceptor(_asyncInterceptor)
        //                .Build();
        //}

        //#endregion // Build_Typed_AsyncInterceptor_Consumer_Test        
    }
}
