namespace EventSourcing.Backbone;


public class ProducerDefaultSegmentationStrategy :
                    IProducerAsyncSegmentationStrategy
{
    ValueTask<Bucket> IProducerAsyncSegmentationStrategy.
            TryClassifyAsync<T>(
                Bucket segments,
                string operation,
                string argumentName,
                T producedData,
                EventSourceOptions options)
    {
        ReadOnlyMemory<byte> data = options.Serializer.Serialize(producedData);
        string key = argumentName;
        return segments.Add(key, data).ToValueTask();
    }
}
