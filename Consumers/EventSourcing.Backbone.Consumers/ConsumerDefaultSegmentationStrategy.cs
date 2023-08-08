namespace EventSourcing.Backbone
{
    /// <summary>
    /// Responsible of converting raw segment (parameter) into object, i.e. deserialize a segment
    /// </summary>
    /// <seealso cref="EventSourcing.Backbone.IConsumerAsyncSegmentationStrategy" />
    public class ConsumerDefaultSegmentationStrategy :
                        IConsumerAsyncSegmentationStrategy
    {
        /// <summary>
        /// Tries to unclassify segment (parameter) into object, i.e. deserialize a segment.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metadata">The metadata.</param>
        /// <param name="segments">The segments.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        ValueTask<(bool, T)> IConsumerAsyncSegmentationStrategy.
                                TryUnclassifyAsync<T>(
                                        Metadata metadata,
                                        Bucket segments,
                                        string argumentName,
                                        EventSourceOptions options)
        {
            if (segments.TryGetValue(argumentName, out ReadOnlyMemory<byte> data))
            {
                try
                {
                    T item = options.Serializer.Deserialize<T>(data);
                    return (true, item).ToValueTask();
                }
                #region Exception Handling

                catch (Exception ex)
                {
                    throw new EventSourcingException($"Fail to serialize event [{metadata}]: argument-name=[{argumentName}], Target type=[{typeof(T).Name}], Base64 Data=[{Convert.ToBase64String(data.ToArray())}]", ex);
                }

                #endregion // Exception Handling
            }

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            var t = default(T);

            return (false, t).ToValueTask();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }
    }
}