using System;

namespace Weknow.EventSource.Backbone
{
    public class EventSourceConsumerOptions: 
        EventSourceOptions, IEventSourceConsumerOptions
    {
        public new static readonly EventSourceConsumerOptions Empty = new EventSourceConsumerOptions();

        public EventSourceConsumerOptions(
            int batchSize = 100,
            TimeoutBehavior timeoutBehavior = TimeoutBehavior.Ack,
            ushort? maxDegreeOfParallelism = null,
            IDataSerializer? serializer = null,
            bool useFullName = false)
            : base(serializer, useFullName)
        {
            BatchSize = batchSize;
            TimeoutBehavior = timeoutBehavior;
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        /// <summary>
        /// Gets the max batch size of reading messages per shard.
        /// The framework won't proceed to the next batch until all messages
        /// in the batch complete (or timeout when it set to acknowledge on timeout).
        /// </summary>
        public int BatchSize { get; }

        /// <summary>
        /// Define the behavior of the framework on timeout.
        /// </summary>
        public TimeoutBehavior TimeoutBehavior { get; }

        /// <summary>
        /// Reduce the maximum concurrency level.
        /// </summary>
        public ushort? MaxDegreeOfParallelism { get; }
    }
}