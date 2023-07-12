using System.Collections.Immutable;

using Polly;

namespace EventSourcing.Backbone
{

    /// <summary>
    /// Common plan properties
    /// </summary>
    public interface IConsumerPlanBase : IPlanBase
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
        /// Gets the configuration.
        /// </summary>
        new ConsumerOptions Options { get; }
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
        /// Gets or sets the invocation resilience policy.
        /// </summary>
        AsyncPolicy ResiliencePolicy { get; }
    }

    /// <summary>
    /// Metadata extensions
    /// </summary>
    public static class IConsumerPlanBaseExtensions
    {
        #region FullUri

        /// <summary>
        /// The stream's full identifier which is a combination of the URI and the environment
        /// </summary>
        public static string FullUri(this IConsumerPlanBase meta, char separator = ':')
        {
            if (string.IsNullOrEmpty(meta.Environment))
                return meta.Uri;
            Env env = meta.Environment;
            string envFormatted = env.Format();
            return $"{envFormatted}{separator}{meta.Uri}";
        }

        #endregion // FullUri
    }

}