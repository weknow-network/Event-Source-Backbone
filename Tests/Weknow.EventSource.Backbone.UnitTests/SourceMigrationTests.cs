
using FakeItEasy;

using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;



namespace Weknow.EventSource.Backbone
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
                        .Partition("Organizations")
                        .Shard("Org: #RedSocks")
                        .BuildSimpleEventProducer();

            await producer.ExecuteAsync("Id", 1);
            await producer.RunAsync(1, DateTime.Now);
            await producer.RunAsync(2, DateTime.Now);

            // ACT
            IRawProducer rawProducer =
               _producerBuilder.UseChannel(_rawProducerChannel)
                       .BuildRaw();

            ISubscriptionBridge subscriptionBridge = new SubscriptionBridge(rawProducer);


            var cts = new CancellationTokenSource();
            IAsyncDisposable subscription =
                 _consumerBuilder.UseChannel(_consumerChannel)
                         //.WithOptions(consumerOptions)r
                         .WithCancellation(cts.Token)
                         .Partition("Organizations")
                         .Shard("Org: #RedSocks")
                         .Subscribe(subscriptionBridge);

            // ASSERT
            Announcement a1 = await _chOfMigration.Reader.ReadAsync();
            Assert.Equal( MessageOrigin.Copy, a1.Metadata.Origin);
            Assert.Equal( MessageOrigin.Original, a1.Metadata.Linked.Origin);
            Assert.Equal( "ExecuteAsync", a1.Metadata.Operation);
            Assert.Equal("Organizations", a1.Metadata.Partition);
            Assert.Equal("Organizations", a1.Metadata.Linked.Partition);
            Assert.Equal("Org: #RedSocks", a1.Metadata.Shard);
            Assert.Equal("Org: #RedSocks", a1.Metadata.Linked.Shard);
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
                        .Partition("Organizations")
                        .Shard("Org: #RedSocks")
                        .BuildSimpleEventProducer();

            await producer.ExecuteAsync("Id", 1);
            await producer.RunAsync(1, DateTime.Now);
            await producer.RunAsync(2, DateTime.Now);

            // ACT
            IRawProducer rawProducer =
               _producerBuilder.UseChannel(_rawProducerChannel)
                       //.WithOptions(producerOption)
                       .Partition("New-Organizations")
                       .Shard("Org: #WhiteSocks")
                       .BuildRaw();

            ISubscriptionBridge subscriptionBridge = new SubscriptionBridge(rawProducer);


            var cts = new CancellationTokenSource();
            IAsyncDisposable subscription =
                 _consumerBuilder.UseChannel(_consumerChannel)
                         //.WithOptions(consumerOptions)r
                         .WithCancellation(cts.Token)
                         .Partition("Organizations")
                         .Shard("Org: #RedSocks")
                         .Subscribe(subscriptionBridge);

            // ASSERT
            Announcement a1 = await _chOfMigration.Reader.ReadAsync();
            Assert.Equal(MessageOrigin.Copy, a1.Metadata.Origin);
            Assert.Equal(MessageOrigin.Original, a1.Metadata.Linked.Origin);
            Assert.Equal( "ExecuteAsync", a1.Metadata.Operation);
            Assert.Equal("New-Organizations", a1.Metadata.Partition);
            Assert.Equal("Organizations", a1.Metadata.Linked.Partition);
            Assert.Equal("Org: #WhiteSocks", a1.Metadata.Shard);
            Assert.Equal("Org: #RedSocks", a1.Metadata.Linked.Shard);
            Assert.Equal(2, a1.Segments.Count());
            Assert.True(a1.Segments.TryGet("key", out string k1));
            Assert.Equal( "Id", k1);
            Assert.True(a1.Segments.TryGet("value", out int v1));
            Assert.Equal( 1, v1);

            Announcement a2 = await _chOfMigration.Reader.ReadAsync();
            Assert.Equal(MessageOrigin.Copy, a2.Metadata.Origin);
            Assert.Equal(MessageOrigin.Original, a2.Metadata.Linked.Origin);
            Assert.Equal("RunAsync", a2.Metadata.Operation);
            Assert.Equal("New-Organizations", a2.Metadata.Partition);
            Assert.Equal("Organizations", a2.Metadata.Linked.Partition);
            Assert.Equal("Org: #WhiteSocks", a2.Metadata.Shard);
            Assert.Equal("Org: #RedSocks", a2.Metadata.Linked.Shard);
            Assert.Equal(2, a1.Segments.Count());
            Assert.True(a2.Segments.TryGet("id", out int i2 ));
            Assert.Equal(1, i2);
            Assert.True(a2.Segments.TryGet("date", out DateTime d2));

            Announcement a3 = await _chOfMigration.Reader.ReadAsync();
            Assert.Equal(MessageOrigin.Copy, a3.Metadata.Origin);
            Assert.Equal(MessageOrigin.Original, a3.Metadata.Linked.Origin);
            Assert.Equal("RunAsync", a3.Metadata.Operation);
            Assert.Equal("New-Organizations", a3.Metadata.Partition);
            Assert.Equal("Organizations", a3.Metadata.Linked.Partition);
            Assert.Equal("Org: #WhiteSocks", a3.Metadata.Shard);
            Assert.Equal("Org: #RedSocks", a3.Metadata.Linked.Shard);
            Assert.Equal(2, a1.Segments.Count());
            Assert.True(a3.Segments.TryGet("id", out int i3 ));
            Assert.Equal(2, i3);
            Assert.True(a3.Segments.TryGet("date", out DateTime d3));

            // CLEANUP
            _ch.Writer.Complete();
            await subscription.DisposeAsync();
            await _ch.Reader.Completion;
        }

        #endregion // Migration_Change_Target_Test


        private class SubscriptionBridge : ISubscriptionBridge
        {
            private readonly IRawProducer _fw;

            public SubscriptionBridge(IRawProducer fw)
            {
                _fw = fw;
            }

            public async Task<bool> BridgeAsync(Announcement announcement, IConsumerBridge consumerBridge)
            {
                await _fw.Produce(announcement);
                return true;

            }
        }
    }
}
