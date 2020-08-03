using System;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{

    public class ProducerDefaultSegmentationStrategy :
                        IProducerAsyncSegmentationStrategy
    {
        private const string PREFIX = "@DEFAULT";

        ValueTask<Bucket> IProducerAsyncSegmentationStrategy.
                TryClassifyAsync<T>(
                    Bucket segments,
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
