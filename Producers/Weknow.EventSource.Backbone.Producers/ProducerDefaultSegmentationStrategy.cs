using System;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{

    public class ProducerDefaultSegmentationStrategy :
                        IProducerAsyncSegmentationStrategy
    {
        ValueTask<Bucket> IProducerAsyncSegmentationStrategy.
                TryClassifyAsync<T>(
                    Bucket segments,
                    string operation, 
                    string argumentName, 
                    T producedData,
                    IEventSourceOptions options)
        {
            ReadOnlyMemory<byte> data = options.Serializer.Serialize(producedData);
            string key = argumentName;
            return segments.Add(key, data).ToValueTask();
        }
    }
}
