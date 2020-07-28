using System;
using System.Threading.Tasks;

using Segments = System.Collections.Immutable.ImmutableDictionary<string, System.ReadOnlyMemory<byte>>;

namespace Weknow.EventSource.Backbone
{
    public class ProducerBase
    {
        private ProducerParameters _parameters;

        public ProducerBase(ProducerParameters parameters)
        {
            _parameters = parameters;
        }

        protected async ValueTask SendAsync<T>(string operation, string argumentName, T producedData)
             where T : notnull
        {
            var segments = Segments.Empty;
            foreach (var strategy in _parameters.SegmentationStrategies)
            {
                segments = await strategy.ClassifyAsync(segments, operation, argumentName, producedData, _parameters.Options);
            }

            var announcment = new Announcement(new Metadata(Guid.NewGuid().ToString(), _parameters.Partition, _parameters.Shard), segments);
            await _parameters.Channel.SendAsync(announcment);
        }
    }
}
