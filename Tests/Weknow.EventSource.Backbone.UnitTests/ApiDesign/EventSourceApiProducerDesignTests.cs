using FakeItEasy;

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;

using Segments = System.Collections.Immutable.ImmutableDictionary<string, System.ReadOnlyMemory<byte>>;

namespace Weknow.EventSource.Backbone
{
    public class EventSourceApiProducerDesignTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IEventSourceProducerChannelBuilder _builder = A.Fake<IEventSourceProducerChannelBuilder>();
        private readonly IProducerChannelProvider _channel = A.Fake<IProducerChannelProvider>();
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

        #region Build_Default_Producer_Test

        [Fact]
        public async Task Build_Default_Producer_Test()
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
                        .Shard("Geek: @someone");
                        //.UseSegmentation<User>((segments, operation, user, opt) =>
                        //{
                        //    var personal = opt.Serializer.Serialize(user.Eracure);
                        //    var open = opt.Serializer.Serialize(user.Details);

                        //    segments = segments.Add(nameof(personal), personal);
                        //    segments =segments.Add(nameof(open), open);

                        //    return segments;
                        //})
                        //.UseSegmentation<int>((segments, operation, id, opt) =>
                        //{
                        //    var data = opt.Serializer.Serialize(id);
                        //    switch (operation)
                        //    {
                        //        case nameof(ISequenceOperationsProducer.LoginAsync):
                        //            segments = segments.Add("log-in", data);
                        //            break;
                        //        case nameof(ISequenceOperationsProducer.EarseAsync):
                        //            segments = segments.Add("clean", data);
                        //            break;
                        //    }

                        //    return segments;
                        //});                        ;


            ISequenceOperationsProducer producer =
                                    _builder
                                        .Merge(producerA, producerB, producerC)
                                        .UseSegmentation(_postSegmentationStrategy)
                                        .Build<ISequenceOperationsProducer>();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);
        }

        #endregion // Build_Default_Producer_Test

        #region Build_Serializer_Producer_Test

        [Fact]
        public async Task Build_Serializer_Producer_Test()
        {
            var option = new EventSourceOptions(_serializer);

            ISequenceOperationsProducer producer =
                _builder.UseChannel(_channel)
                        .WithOptions(option)
                        .Partition("Organizations")
                        .Shard("Org: #RedSocks")
                        .Build<ISequenceOperationsProducer>();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);
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
