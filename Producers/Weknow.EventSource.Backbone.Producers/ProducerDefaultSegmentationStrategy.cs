using System;
using System.Collections;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{

    public class ProducerDefaultSegmentationStrategy :
                        IProducerAsyncSegmentationStrategy
    {
        private const string PREFIX = "@DEFAULT";

        ValueTask<ImmutableDictionary<string, ReadOnlyMemory<byte>>> IProducerAsyncSegmentationStrategy.
                TryClassifyAsync<T>(
                    ImmutableDictionary<string, ReadOnlyMemory<byte>> segments,
                    string operation, 
                    string argumentName, 
                    T producedData,
                    IEventSourceOptions options)
        {
            ReadOnlyMemory<byte> data = options.Serializer.Serialize(producedData);
            string key = $"{PREFIX}~{operation}~{argumentName}";
            return segments.Add(key, data).ToValueTask();
        }
    }
}
