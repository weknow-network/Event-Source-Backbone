using System;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    public class ConsumerDefaultSegmentationStrategy :
                        IConsumerAsyncSegmentationStrategy
    {
        ValueTask<(bool, T)> IConsumerAsyncSegmentationStrategy.
                                TryUnclassifyAsync<T>(
                                        Bucket segments,
                                        string operation,
                                        string argumentName,
                                        EventSourceOptions options)
        {
            string key = argumentName;
            if (segments.TryGetValue(key, out ReadOnlyMemory<byte> data))
            {
                T item = options.Serializer.Deserialize<T>(data);
                return (true, item).ToValueTask();
            }

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            var t = default(T);

            return (false, t).ToValueTask();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        }
    }
}