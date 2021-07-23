using Microsoft.Extensions.Logging;

using Polly;

using System;
using System.Collections.Immutable;
using System.Threading;

namespace Weknow.EventSource.Backbone
{

    /// <summary>
    /// Common plan properties
    /// </summary>
    public interface IConsumerPlanBase
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
        /// <summary>
        /// Gets the consumer group.
        /// Consumer Group allow a group of clients to cooperate
        /// consuming a different portion of the same stream of messages
        /// </summary>
        string ConsumerGroup { get; }
        /// <summary>
        /// Optional Name of the consumer.
        /// Can use for observability.
        /// </summary>
        string ConsumerName { get; }
        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        CancellationToken Cancellation { get; }
        /// <summary>
        /// Gets the storage strategy.
        /// </summary>
        ImmutableArray<IConsumerStorageStrategyWithFilter> StorageStrategy { get; }
        /// <summary>
        /// Consumer interceptors (Timing: after serialization).
        /// </summary>
        /// <value>
        /// The interceptors.
        /// </value>
        IImmutableList<IConsumerAsyncInterceptor> Interceptors { get; }
        /// <summary>
        /// Gets the logger.
        /// </summary>
        ILogger Logger { get; }
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        IEventSourceConsumerOptions Options { get; }
        /// <summary>
        /// Segmentation responsible of splitting an instance into segments.
        /// Segments is how the Consumer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        IImmutableList<IConsumerAsyncSegmentationStrategy> SegmentationStrategies { get; }

        /// <summary>
        /// Attach the shard.
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <returns></returns>
        IConsumerPlan WithShard(string shard);

        /// <summary>
        /// Gets or sets the invocation resilience policy.
        /// </summary>
        AsyncPolicy ResiliencePolicy { get; }

    }
}