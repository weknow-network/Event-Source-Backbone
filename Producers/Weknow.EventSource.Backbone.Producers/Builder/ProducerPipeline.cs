using System;
using System.Collections;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.CodeGeneration;

using Segments = System.Collections.Immutable.ImmutableDictionary<string, System.ReadOnlyMemory<byte>>;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Base functionality for the producer pipeline
    /// </summary>
    internal class ProducerPipeline
    {
        private readonly IProducerChannelProvider _channel;
        private readonly IImmutableList<IProducerAsyncSegmentationStrategy> _segmentationStrategy;
        private readonly IEventSourceOptions _options;
        private readonly string _partition;
        private readonly string _shard;

        public ProducerPipeline(
            ProducerParameters parameters)
        {
            _channel = parameters.Channel;
            _options = parameters.Options;
            _segmentationStrategy = parameters.SegmentationStrategies;
        }

        internal async ValueTask SendAsync<T>(
            T argumentValue,
            string operation,
            string argumentName)
        { 
            string id = Guid.NewGuid().ToString();
            var meta = new Metadata(id, _partition, _shard);
            Segments segments = Segments.Empty;
            foreach (var strategy in _segmentationStrategy)
            {
                segments = await strategy.ClassifyAsync<T>(segments, operation, argumentName, argumentValue, _options);
            }

            var announcment = new Announcement(meta, segments);
            await _channel.SendAsync(announcment);
        }

        protected async ValueTask SendAsync(Segments segments)
        {
            throw new NotImplementedException("Should get the metadata & segmentation decorator from the PIPELINE");
            //string id = Guid.NewGuid().ToString();
            //var meta = new Metadata(id, "Orders", "Order #53662332");
            //var announcment = new Announcement(meta, segments);
            //await _channel.SendAsync(announcment);
        }
    }
}
