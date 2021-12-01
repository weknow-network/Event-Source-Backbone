using Microsoft.Extensions.Logging;

using Polly;

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{

    /// <summary>
    /// Common plan properties
    /// </summary>
    public interface IConsumerPlanBase: IPlanRoute
    {
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
        ConsumerOptions Options { get; }
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

    /// <summary>
    /// Metadata extensions
    /// </summary>
    public static class IConsumerPlanBaseExtensions
    {
        #region Key

        /// <summary>
        /// Gets the partition:shard as key.
        /// </summary>
        public static string Key(this IConsumerPlanBase meta, char separator = ':')
        {
            if (string.IsNullOrEmpty(meta.Environment))
                return $"{meta.Partition}{separator}{meta.Shard}";
            Env env = meta.Environment;
            string envFormatted = env.Format();
            return $"{envFormatted}{separator}{meta.Partition}{separator}{meta.Shard}";
        }

        #endregion // Key
    }

}