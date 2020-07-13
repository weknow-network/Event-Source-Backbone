
using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Enable configuration.
    /// </summary>
    public interface IEventSourceProducerPartitionBuilder
    {
        /// <summary>
        /// Partition key represent logical group of 
        /// event source shards.
        /// For example assuming each ORDERING flow can have its 
        /// own messaging sequence, yet can live concurrency with 
        /// other ORDER's sequences.
        /// The partition will let consumer the option to be notify and
        /// consume multiple shards from single consumer.
        /// This way the consumer can handle all orders in
        /// central place without affecting sequence of specific order 
        /// flow or limiting the throughput.
        /// </summary>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns></returns>
        IEventSourceProducerShardBuilder Partition(string partitionKey);
    }
}
