using Microsoft.Extensions.Logging;

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{

    /// <summary>
    /// Hold builder definitions.
    /// Define the consumer execution pipeline.
    /// </summary>
    public class ConsumerPlan
    {
        public static readonly ConsumerPlan Empty = new ConsumerPlan();

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        private ConsumerPlan()
        {
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="copyFrom">The copy from.</param>
        /// <param name="channel">The channel.</param>
        /// <param name="partition">The partition.</param>
        /// <param name="shard">The shard.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The options.</param>
        /// <param name="segmentationStrategies">The segmentation strategies.</param>
        /// <param name="interceptors">The interceptors.</param>
        /// <param name="routes">The routes.</param>
        /// <param name="cancellations">The cancellations.</param>
        private ConsumerPlan(
            ConsumerPlan copyFrom,
            IConsumerChannelProvider? channel = null,
            string? partition = null,
            string? shard = null,
            ILogger? logger = null,
            IEventSourceConsumerOptions? options = null,
            IImmutableList<IConsumerAsyncSegmentationStrategy>? segmentationStrategies = null,
            IImmutableList<IConsumerAsyncInterceptor>? interceptors = null,
            IImmutableList<IConsumerHooksBuilder>? routes = null,
            IImmutableList<CancellationToken>? cancellations = null)
        {
            Channel = channel ?? copyFrom.Channel;
            Partition = partition ?? copyFrom.Partition;
            Shard = shard ?? copyFrom.Shard;
            Logger = logger;
            Options = options ?? copyFrom.Options;
            SegmentationStrategies = segmentationStrategies ?? copyFrom.SegmentationStrategies;
            Interceptors = interceptors ?? copyFrom.Interceptors;
            Routes = routes ?? copyFrom.Routes;
            Cancellations = cancellations ?? copyFrom.Cancellations;
        }

        #endregion // Ctor

        #region Channel

        /// <summary>
        /// Gets the communication channel provider.
        /// </summary>
        public IConsumerChannelProvider Channel { get; } = NopChannel.Empty;

        #endregion // Channel

        #region Logger

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public ILogger? Logger { get; } = null;

        #endregion // Channel

        #region Partition

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
        public string Partition { get; } = string.Empty;

        #endregion // Partition

        #region Shard

        /// <summary>
        /// Shard key represent physical sequence.
        /// Use same shard when order is matter.
        /// For example: assuming each ORDERING flow can have its 
        /// own messaging sequence, in this case you can split each 
        /// ORDER into different shard and gain performance bust..
        /// </summary>
        public string Shard { get; } = string.Empty;

        #endregion // Shard

        #region Options

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IEventSourceConsumerOptions Options { get; } = EventSourceConsumerOptions.Empty;

        #endregion // Options

        #region SegmentationStrategies

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
        public IImmutableList<IConsumerAsyncSegmentationStrategy> SegmentationStrategies { get; } =
                    ImmutableList<IConsumerAsyncSegmentationStrategy>.Empty;

        #endregion // SegmentationStrategies

        #region Interceptors

        /// <summary>
        /// Consumer interceptors (Timing: after serialization).
        /// </summary>
        /// <value>
        /// The interceptors.
        /// </value>
        public IImmutableList<IConsumerAsyncInterceptor> Interceptors { get; } =
                    ImmutableList<IConsumerAsyncInterceptor>.Empty;

        #endregion // Interceptors

        #region Cancellations

        /// <summary>
        /// Gets the cancellation tokens.
        /// </summary>
        public IImmutableList<CancellationToken> Cancellations { get; } = ImmutableList<CancellationToken>.Empty;

        #endregion // Cancellations

        #region Routes

        /// <summary>
        /// Routes are sub-pipelines are results of merge operation
        /// which can split same payload into multiple partitions or shards.
        /// </summary>
        private readonly IImmutableList<IConsumerHooksBuilder> Routes =
                ImmutableList<IConsumerHooksBuilder>.Empty;

        #endregion // Routes

        //------------------------------------------

        #region WithChannel

        /// <summary>
        /// Attach the channel.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <returns></returns>
        internal ConsumerPlan WithChannel(IConsumerChannelProvider channel)
        {
            return new ConsumerPlan(this, channel: channel);
        }

        #endregion // WithChannel

        #region WithLogger

        /// <summary>
        /// Attach the LOGGER.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        internal ConsumerPlan WithLogger(ILogger logger)
        {
            return new ConsumerPlan(this, logger: logger);
        }

        #endregion // WithChannel

        #region WithCancellation

        /// <summary>
        /// Attach the cancellation.
        /// </summary>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        internal ConsumerPlan WithCancellation(
                                        CancellationToken cancellation)
        {
            var cancellations = Cancellations.Add(cancellation);
            return new ConsumerPlan(this, cancellations: cancellations);
        }

        #endregion // WithCancellation

        #region WithOptions

        /// <summary>
        /// Attach the options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        internal ConsumerPlan WithOptions(
                                        IEventSourceConsumerOptions options)
        {
            return new ConsumerPlan(this, options: options);
        }

        #endregion // WithOptions

        #region WithPartition

        /// <summary>
        /// Attach the partition.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <returns></returns>
        internal ConsumerPlan WithPartition(
                                                string partition)
        {
            return new ConsumerPlan(this, partition: partition);
        }

        #endregion // WithPartition

        #region WithShard

        /// <summary>
        /// Attach the shard.
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <returns></returns>
        internal ConsumerPlan WithShard(
                                                string shard)
        {
            return new ConsumerPlan(this, shard: shard);
        }

        #endregion // WithShard

        #region AddRoute

        /// <summary>
        /// Adds the route.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns></returns>
        internal ConsumerPlan AddRoute(
                                                IConsumerHooksBuilder route)
        {
            return new ConsumerPlan(this, routes: Routes.Add(route));
        }

        #endregion // AddRoute

        #region AddSegmentation

        /// <summary>
        /// Adds the segmentation.
        /// </summary>
        /// <param name="segmentation">The segmentation.</param>
        /// <returns></returns>
        internal ConsumerPlan AddSegmentation(
                                                IConsumerAsyncSegmentationStrategy segmentation)
        {
            return new ConsumerPlan(this,
                            segmentationStrategies: SegmentationStrategies.Add(segmentation));
        }

        #endregion // AddSegmentation

        #region AddInterceptor

        /// <summary>
        /// Adds the interceptor.
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns></returns>
        internal ConsumerPlan AddInterceptor(
                                  IConsumerAsyncInterceptor interceptor)
        {
            return new ConsumerPlan(this,
                            interceptors: Interceptors.Add(interceptor));
        }

        #endregion // AddInterceptor

        #region class NopChannel

        /// <summary>
        /// Not operational channel
        /// </summary>
        /// <seealso cref="Weknow.EventSource.Backbone.IConsumerChannelProvider" />
        private class NopChannel : IConsumerChannelProvider
        {
            public static readonly IConsumerChannelProvider Empty = new NopChannel();

            /// <summary>
            /// Receives the asynchronous.
            /// </summary>
            /// <param name="func">The function.</param>
            /// <param name="options">The options.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns></returns>
            /// <exception cref="NotSupportedException">Channel must be define</exception>
            public ValueTask ReceiveAsync(
                Func<Announcement, ValueTask> func,
                IEventSourceConsumerOptions options,
                CancellationToken cancellationToken)
            {
                throw new NotSupportedException("Channel must be define");
            }
        }

        #endregion // class NopChannel    }
    }
}
