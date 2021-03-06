﻿using Microsoft.Extensions.Logging;

using Polly;

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.Private;

namespace Weknow.EventSource.Backbone
{

    /// <summary>
    /// Hold builder definitions.
    /// Define the consumer execution pipeline.
    /// </summary>
    public class ConsumerPlan : IConsumerPlan
    {
        public static readonly ConsumerPlan Empty = new ConsumerPlan();

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        private ConsumerPlan()
        {
            ResiliencePolicy = Policy.Handle<Exception>()
                          .WaitAndRetryAsync(3,
                                (retryCount) => TimeSpan.FromSeconds(Math.Pow(2, retryCount)),
                                 ((ex, duration, retryCount, ctx) => Logger?.LogWarning(ex,"Retry {count}", retryCount)));

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
        /// <param name="cancellation">The cancellation token.</param>
        /// <param name="consumerGroup">Consumer Group allow a group of clients to cooperate
        /// consuming a different portion of the same stream of messages</param>
        /// <param name="consumerName">Optional Name of the consumer.
        /// Can use for observability.</param>
        /// <param name="resiliencePolicy">The resilience policy.</param>
        /// <param name="storageStrategy">The storage strategy.</param>
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
            CancellationToken? cancellation = null,
            string? consumerGroup = null,
            string? consumerName = null,
            AsyncPolicy? resiliencePolicy = null,
            IConsumerStorageStrategyWithFilter? storageStrategy = null)
        {
            Channel = channel ?? copyFrom.Channel;
            Partition = partition ?? copyFrom.Partition;
            Shard = shard ?? copyFrom.Shard;
            Logger = logger ?? copyFrom.Logger;
            Options = options ?? copyFrom.Options;
            SegmentationStrategies = segmentationStrategies ?? copyFrom.SegmentationStrategies;
            Interceptors = interceptors ?? copyFrom.Interceptors;
            Routes = routes ?? copyFrom.Routes;
            Cancellation = cancellation ?? copyFrom.Cancellation;
            ConsumerGroup = consumerGroup ?? copyFrom.ConsumerGroup;
            ConsumerName = consumerName ?? copyFrom.ConsumerName;
            ResiliencePolicy = resiliencePolicy ?? copyFrom.ResiliencePolicy;
            StorageStrategy = storageStrategy == null
                  ? copyFrom.StorageStrategy
                  : copyFrom.StorageStrategy.Add(storageStrategy);
        }

        #endregion // Ctor

        #region Channel

        /// <summary>
        /// Gets the communication channel provider.
        /// </summary>
        public IConsumerChannelProvider Channel { get; } = NopChannel.Empty;

        #endregion // Channel

        #region StorageStrategy

        /// <summary>
        /// Gets the storage strategy.
        /// </summary>
        public ImmutableArray<IConsumerStorageStrategyWithFilter> StorageStrategy { get; } = ImmutableArray<IConsumerStorageStrategyWithFilter>.Empty;

        #endregion // StorageStrategy

        #region Logger

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public ILogger Logger { get; } = EventSourceFallbakLogger.Default;

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
        public IEventSourceConsumerOptions Options { get; } = ConsumerOptions.Empty;

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

        #region Cancellation

        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        public CancellationToken Cancellation { get; } = default;

        #endregion // Cancellation

        #region Routes

        /// <summary>
        /// Routes are sub-pipelines are results of merge operation
        /// which can split same payload into multiple partitions or shards.
        /// </summary>
        private readonly IImmutableList<IConsumerHooksBuilder> Routes =
                ImmutableList<IConsumerHooksBuilder>.Empty;

        #endregion // Routes

        #region ConsumerGroup

        /// <summary>
        /// Gets the consumer group.
        /// Consumer Group allow a group of clients to cooperate
        /// consuming a different portion of the same stream of messages
        /// </summary>
        public string ConsumerGroup { get; } = string.Empty;

        #endregion // ConsumerGroup

        #region ConsumerName

        /// <summary>
        /// Optional Name of the consumer.
        /// Can use for observability.
        /// </summary>
        public string ConsumerName { get; } = Guid.NewGuid().ToString("N");

        #endregion // ConsumerName

        #region ResiliencePolicy

        /// <summary>
        /// Gets or sets the invocation resilience policy.
        /// </summary>
        public AsyncPolicy ResiliencePolicy { get; }

        #endregion // ResiliencePolicy

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

        #region WithStorageStrategy

        /// <summary>
        /// Attach the Storage Strategy.
        /// </summary>
        /// <param name="storageStrategy">The storage strategy.</param>
        /// <returns></returns>
        internal ConsumerPlan WithStorageStrategy(IConsumerStorageStrategyWithFilter storageStrategy)
        {
            return new ConsumerPlan(this, storageStrategy: storageStrategy);
        }

        #endregion // WithStorageStrategy

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
            
            if(Cancellation == default)
                return new ConsumerPlan(this, cancellation: cancellation);

            var cts = CancellationTokenSource.CreateLinkedTokenSource(Cancellation, cancellation);
            return new ConsumerPlan(this, cancellation: cts.Token);

        }

        #endregion // WithCancellation

        #region WithOptions

        /// <summary>
        /// Attach the options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        internal ConsumerPlan WithOptions(IEventSourceConsumerOptions options)
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
        internal ConsumerPlan WithPartition(string partition)
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
        IConsumerPlan IConsumerPlan.WithShard(string shard) => WithShard(shard);
        /// <summary>
        /// Attach the shard.
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <returns></returns>
        internal ConsumerPlan WithShard(string shard)
        {
            return new ConsumerPlan(this, shard: shard);
        }

        #endregion // WithShard

        #region WithResiliencePolicy

        /// <summary>
        /// Set resilience policy
        /// </summary>
        /// <param name="policy">The policy.</param>
        /// <returns></returns>
        internal ConsumerPlan WithResiliencePolicy(AsyncPolicy policy)
        {
            return new ConsumerPlan(this, resiliencePolicy: policy);
        }

        #endregion // WithResiliencePolicy

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

        #region ConsumerGroup

        /// <summary>
        /// Set Consumer Group which allow a group of clients to cooperate
        /// consuming a different portion of the same stream of messages
        /// </summary>
        /// <param name="consumerGroup">
        /// Consumer Group allow a group of clients to cooperate
        /// consuming a different portion of the same stream of messages
        /// </param>
        /// <param name="consumerName">
        /// Optional Name of the consumer.
        /// Can use for observability.
        /// </param>
        /// <returns></returns>
        internal ConsumerPlan WithConsumerGroup(
            string consumerGroup,
            string? consumerName = null) => new ConsumerPlan(this,
                                                consumerGroup: consumerGroup,
                                                consumerName: consumerName);

        #endregion // ConsumerGroup

        #region class NopChannel

        /// <summary>
        /// Not operational channel
        /// </summary>
        /// <seealso cref="Weknow.EventSource.Backbone.IConsumerChannelProvider" />
        private class NopChannel : IConsumerChannelProvider
        {
            public static readonly IConsumerChannelProvider Empty = new NopChannel();

            /// <summary>
            /// Subscribe to the channel for specific metadata.
            /// </summary>
            /// <param name="plan">The consumer plan.</param>
            /// <param name="func">The function.</param>
            /// <param name="options">The options.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>
            /// When completed
            /// </returns>
            /// <exception cref="NotSupportedException">Channel must be define</exception>
            public ValueTask SubsribeAsync(
                    IConsumerPlan plan,
                    Func<Announcement, IAck, ValueTask> func,
                    IEventSourceConsumerOptions options,
                    CancellationToken cancellationToken)
            {
                throw new NotSupportedException("Channel must be define");
            }
        }

        #endregion // class NopChannel    }
    }
}
