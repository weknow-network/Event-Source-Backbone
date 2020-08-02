using System;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    public class ConsumerDefaultSegmentationStrategy :
                        IConsumerAsyncSegmentationStrategy
    {
        private const string PREFIX = "@DEFAULT";

        ValueTask<(bool, T)> IConsumerAsyncSegmentationStrategy.
                                TryUnclassifyAsync<T>(
                                        Bucket segments,
                                        string operation,
                                        string argumentName,
                                        IEventSourceOptions options)
        {
            string key = $"{PREFIX}~{operation}~{argumentName}";
            if (segments.TryGetValue(key, out ReadOnlyMemory<byte> data))
            {
                T item = options.Serializer.Deserialize<T>(data);
                return (true, item).ToValueTask();
            }
            return (false, default(T)).ToValueTask();
        }
    }
}