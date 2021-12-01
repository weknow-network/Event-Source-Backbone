using System;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Plan routing identification
    /// </summary>
    public interface IPlanRoute
    {
        /// <summary>
        /// Environment (part of the stream key).
        /// </summary>
        /// <value>
        /// The partition.
        /// </value>
        Env Environment { get; }
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
        /// <value>
        /// The partition.
        /// </value>
        string Partition { get; }
        /// <summary>
        /// Shard key represent physical sequence.
        /// Use same shard when order is matter.
        /// For example: assuming each ORDERING flow can have its 
        /// own messaging sequence, in this case you can split each 
        /// ORDER into different shard and gain performance bust..
        /// </summary>
        string Shard { get; }

    }
}