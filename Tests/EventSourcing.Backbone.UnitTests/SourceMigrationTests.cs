
using System.Threading.Channels;

using EventSourcing.Backbone.Building;
using EventSourcing.Backbone.UnitTests.Entities;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;



namespace EventSourcing.Backbone
{
    public class SourceMigrationTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IProducerBuilder _producerBuilder = ProducerBuilder.Empty;
        private readonly IConsumerBuilder _consumerBuilder = ConsumerBuilder.Empty;
        private readonly Func<ILogger, IProducerChannelProvider> _producerChannel;
        private readonly Func<ILogger, IProducerChannelProvider> _rawProducerChannel;
        private readonly Func<ILogger, IConsumerChannelProvider> _consumerChannel;
        // private readonly IDataSerializer _serializer;
        private readonly IProducerInterceptor _rawInterceptor = A.Fake<IProducerInterceptor>();
        private readonly IProducerAsyncInterceptor _rawAsyncInterceptor = A.Fake<IProducerAsyncInterceptor>();
        private readonly IProducerAsyncSegmentationStrategy _segmentationStrategy = A.Fake<IProducerAsyncSegmentationStrategy>();
        private readonly IProducerSegmentationStrategy _otherSegmentationStrategy = A.Fake<IProducerSegmentationStrategy>();
        private readonly IProducerSegmentationStrategy _postSegmentationStrategy = A.Fake<IProducerSegmentationStrategy>();

        private readonly Channel<Announcement> _ch;
        private readonly Channel<Announcement> _chOfMigration;

        #region Ctor

        public SourceMigrationTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            // _serializer = new JsonDataSerializer();
            _ch = Channel.CreateUnbounded<Announcement>();
            _chOfMigration = Channel.CreateUnbounded<Announcement>();
            _producerChannel = _ => new ProducerTestChannel(_ch);
            _rawProducerChannel = _ => new ProducerTestChannel(_chOfMigration);
            _consumerChannel = _ => new ConsumerTestChannel(_ch);
        }

        #endregion // Ctor

        #region Migration_Simple_Test

        [Fact]
        public async Task Migration_Simple_Test()
        {
            // PREPARE
            ISimpleEventProducer producer =
                _producerBuilder.UseChannel(_producerChannel)
                        //.WithOptions(producerOption)
                        .Uri("Kids:HappySocks")
                        .BuildSimpleEventProducer();

            await producer.ExecuteAsync("Id", 1);
            await producer.RunAsync(1, DateTime.Now);
            await producer.RunAsync(2, DateTime.Now);

            // ACT
            IRawProducer rawProducer =
               _producerBuilder.UseChannel(_rawProducerChannel)
                       .BuildRaw();

            ISubscriptionBridge subscriptionBridge = rawProducer.ToSubscriptionBridge();


            var cts = new CancellationTokenSource();
            IAsyncDisposable subscription =
                 _consumerBuilder.UseChannel(_consumerChannel)
                         //.WithOptions(consumerOptions)r
                         .WithCancellation(cts.Token)
                         .Uri("Kids:HappySocks")
                         .Subscribe(subscriptionBridge);

            // ASSERT
            Announcement a1 = await _chOfMigration.Reader.ReadAsync();
            Assert.Equal(MessageOrigin.Copy, a1.Metadata.Origin);
            Assert.Equal(MessageOrigin.Original, a1.Metadata.Linked.Origin);
            Assert.Equal("ExecuteAsync", a1.Metadata.Signature.Operation);
            Assert.Equal("Kids:HappySocks", a1.Metadata.Uri);
            Assert.Equal("Kids:HappySocks", a1.Metadata.Linked.Uri);
            Assert.Equal(2, a1.Segments.Count());

            Assert.True(a1.Segments.TryGet("key", out string k1));
            Assert.Equal("Id", k1);
            Assert.True(a1.Segments.TryGet("value", out int v1));
            Assert.Equal(1, v1);

            Announcement a2 = await _chOfMigration.Reader.ReadAsync();
            Assert.Equal(MessageOrigin.Copy, a2.Metadata.Origin);
            Assert.Equal(MessageOrigin.Original, a2.Metadata.Linked.Origin);
            Assert.Equal(2, a1.Segments.Count());
            Assert.True(a2.Segments.TryGet("id", out int i2));
            Assert.Equal(1, i2);
            Assert.True(a2.Segments.TryGet("date", out DateTime d2));

            Announcement a3 = await _chOfMigration.Reader.ReadAsync();
            Assert.Equal(MessageOrigin.Copy, a3.Metadata.Origin);
            Assert.Equal(MessageOrigin.Original, a3.Metadata.Linked.Origin);
            Assert.Equal(2, a1.Segments.Count());
            Assert.True(a3.Segments.TryGet("id", out int i3));
            Assert.Equal(2, i3);
            Assert.True(a3.Segments.TryGet("date", out DateTime d3));

            // CLEANUP
            _ch.Writer.Complete();
            await subscription.DisposeAsync();
            await _ch.Reader.Completion;
        }

        #endregion // Migration_Simple_Test

        #region Migration_Change_Target_Test

        [Fact]
        public async Task Migration_Change_Target_Test()
        {
            // PREPARE
            ISimpleEventProducer producer =
                _producerBuilder.UseChannel(_producerChannel)
                        //.WithOptions(producerOption)
                        .Uri("Kids:HappySocks")
                        .BuildSimpleEventProducer();

            await producer.ExecuteAsync("Id", 1);
            await producer.RunAsync(1, DateTime.Now);
            await producer.RunAsync(2, DateTime.Now);

            // ACT
            IRawProducer rawProducer =
               _producerBuilder.UseChannel(_rawProducerChannel)
                       //.WithOptions(producerOption)
                       .Uri("Man:Socks")
                       .BuildRaw();

            ISubscriptionBridge subscriptionBridge = rawProducer.ToSubscriptionBridge();


            var cts = new CancellationTokenSource();
            IAsyncDisposable subscription =
                 _consumerBuilder.UseChannel(_consumerChannel)
                         //.WithOptions(consumerOptions)r
                         .WithCancellation(cts.Token)
                         .Uri("Kids:HappySocks")
                         .Subscribe(subscriptionBridge);

            // ASSERT
            Announcement a1 = await _chOfMigration.Reader.ReadAsync();
            Assert.Equal(MessageOrigin.Copy, a1.Metadata.Origin);
            Assert.Equal(MessageOrigin.Original, a1.Metadata.Linked.Origin);
            Assert.Equal("ExecuteAsync", a1.Metadata.Signature.Operation);
            Assert.Equal("Man:Socks", a1.Metadata.Uri);
            Assert.Equal("Kids:HappySocks", a1.Metadata.Linked.Uri);
            Assert.Equal(2, a1.Segments.Count());
            Assert.True(a1.Segments.TryGet("key", out string k1));
            Assert.Equal("Id", k1);
            Assert.True(a1.Segments.TryGet("value", out int v1));
            Assert.Equal(1, v1);

            Announcement a2 = await _chOfMigration.Reader.ReadAsync();
            Assert.Equal(MessageOrigin.Copy, a2.Metadata.Origin);
            Assert.Equal(MessageOrigin.Original, a2.Metadata.Linked.Origin);
            Assert.Equal("RunAsync", a2.Metadata.Signature.Operation);
            Assert.Equal("Man:Socks", a2.Metadata.Uri);
            Assert.Equal("Kids:HappySocks", a2.Metadata.Linked.Uri);
            Assert.Equal(2, a1.Segments.Count());
            Assert.True(a2.Segments.TryGet("id", out int i2));
            Assert.Equal(1, i2);
            Assert.True(a2.Segments.TryGet("date", out DateTime d2));

            Announcement a3 = await _chOfMigration.Reader.ReadAsync();
            Assert.Equal(MessageOrigin.Copy, a3.Metadata.Origin);
            Assert.Equal(MessageOrigin.Original, a3.Metadata.Linked.Origin);
            Assert.Equal("RunAsync", a3.Metadata.Signature.Operation);
            Assert.Equal("Man:Socks", a3.Metadata.Uri);
            Assert.Equal("Kids:HappySocks", a3.Metadata.Linked.Uri);
            Assert.Equal(2, a1.Segments.Count());
            Assert.True(a3.Segments.TryGet("id", out int i3));
            Assert.Equal(2, i3);
            Assert.True(a3.Segments.TryGet("date", out DateTime d3));

            // CLEANUP
            _ch.Writer.Complete();
            await subscription.DisposeAsync();
            await _ch.Reader.Completion;
        }

        #endregion // Migration_Change_Target_Test
    }
}
