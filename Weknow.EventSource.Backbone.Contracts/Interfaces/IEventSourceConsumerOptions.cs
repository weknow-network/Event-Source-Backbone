using System;

namespace Weknow.EventSource.Backbone
{
    public interface IEventSourceConsumerOptions: IEventSourceOptions
    {

        /// <summary>
        /// Gets the max batch size of reading messages per shard.
        /// The framework won't proceed to the next batch until all messages
        /// in the batch complete (or timeout when it set to acknowledge on timeout).
        /// </summary>
        int BatchSize { get; }

        /// <summary>
        /// Define the behavior of the framework on timeout.
        /// </summary>
        TimeoutBehavior TimeoutBehavior { get; }

        /// <summary>
        /// Reduce the maximum concurrency level.
        /// </summary>
        ushort? MaxDegreeOfParallelism { get; }
    }
}