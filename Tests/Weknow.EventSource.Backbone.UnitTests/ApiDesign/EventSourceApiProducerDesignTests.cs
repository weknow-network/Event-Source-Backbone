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
                        .ForEventsSequence<ISequenceOperations>();

            var producerB =
                _builder.UseTestProducerChannel()
                        .Partition("NGOs")
                        .Shard("NGO #2782228")
                        .ForEventsSequence<ISequenceOperations>();

            var producerC =
                _builder.UseChannel(_channel)
                        .Partition("Fans")
                        .Shard("Geek: @someone")
                        .ForEventsSequence<ISequenceOperations>();


            ISequenceOperations producer = _builder
                                                    .Merge(producerA, producerB, producerC)
                                                    .AddSegmentationProvider((opt, store) =>
                                                        new SequenceOperationsSegmentation(opt, store))
                                                    .Build();

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

            ISequenceOperations producer =
                _builder.UseChannel(_channel)
                        .WithOptions(option)
                        .Partition("Organizations")
                        .Shard("Org: #RedSocks")
                        .ForEventsSequence<ISequenceOperations>()
                        .Build();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);
        }

        #endregion // Build_Serializer_Producer_Test

        #region Build_Interceptor_Producer_Test

        [Fact]
        public async Task Build_Interceptor_Producer_Test()
        {
            ISequenceOperations producer =
                _builder.UseChannel(_channel)
                        .Partition("Organizations")
                        .Shard("Org: #RedSocks")
                        .AddInterceptor(_rawInterceptor)
                        .AddAsyncInterceptor(_rawAsyncInterceptor)
                        .ForEventsSequence<ISequenceOperations>()
                        .Build();

            await producer.RegisterAsync(new User());
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(4335);
        }

        #endregion // Build_Interceptor_Producer_Test
    }
}
