using System;
using System.Collections;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{

    /// <summary>
    /// This class is a bucket of parameters which built-up 
    /// by the producer builder.
    /// It will used to define the producer execution pipeline.
    /// </summary>
    public class ProducerParameters
    {       
        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal ProducerParameters(IProducerChannelProvider channel)
        {
            Channel = channel;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="copyFrom">The copy from.</param>
        /// <param name="channel">The channel.</param>
        /// <param name="partition">The partition.</param>
        /// <param name="shard">The shard.</param>
        /// <param name="options">The options.</param>
        /// <param name="segmentationStrategies">The segmentation strategies.</param>
        /// <param name="interceptors">The interceptors.</param>
        /// <param name="routes">The routes.</param>
        private ProducerParameters(
            ProducerParameters copyFrom,
            IProducerChannelProvider? channel = null,
            string? partition = null,
            string? shard = null,
            IEventSourceOptions? options = null,
            IImmutableList<IProducerAsyncSegmentationStrategy>? segmentationStrategies = null,
            IImmutableList<IProducerAsyncInterceptor>? interceptors = null,
            IImmutableList<IProducerHooksBuilder>? routes = null)
        {
            Channel = channel ?? copyFrom.Channel;
            Partition = partition ?? copyFrom.Partition;
            Shard = shard ?? copyFrom.Shard;
            Options = options ?? copyFrom.Options;
            SegmentationStrategies = segmentationStrategies ?? copyFrom.SegmentationStrategies;
            Interceptors = interceptors ?? copyFrom.Interceptors;
            Routes = routes ?? copyFrom.Routes;
        }

        #endregion // Ctor

        #region Channel

        /// <summary>
        /// Gets the communication channel provider.
        /// </summary>
        public IProducerChannelProvider Channel { get; } // TODO: [bnaya, 2020-07] assign in memory channel provider

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
        public IEventSourceOptions Options { get; } = EventSourceOptions.Empty;

        #endregion // Options

        #region SegmentationStrategies

        /// <summary>
        /// Segmentation responsible of splitting an instance into segments.
        /// Segments is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        public IImmutableList<IProducerAsyncSegmentationStrategy> SegmentationStrategies { get; } =
                    ImmutableList<IProducerAsyncSegmentationStrategy>.Empty;

        #endregion // SegmentationStrategies

        #region Interceptors

        /// <summary>
        /// Producer interceptors (Timing: after serialization).
        /// </summary>
        /// <value>
        /// The interceptors.
        /// </value>
        public IImmutableList<IProducerAsyncInterceptor> Interceptors { get; } =
                    ImmutableList<IProducerAsyncInterceptor>.Empty;

        #endregion // Interceptors

        #region Routes

        /// <summary>
        /// Routes are sub-pipelines are results of merge operation
        /// which can split same payload into multiple partitions or shards.
        /// </summary>
        private readonly IImmutableList<IProducerHooksBuilder> Routes =
                ImmutableList<IProducerHooksBuilder>.Empty;

        #endregion // Routes

        //---------------------------------------

        #region WithOptions

        /// <summary>
        /// Withes the options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        internal ProducerParameters WithOptions(
                                        IEventSourceOptions options)
        {
            return new ProducerParameters(this, options: options);
        }

        #endregion // WithOptions

        #region WithPartition

        /// <summary>
        /// Withes the partition.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <returns></returns>
        internal ProducerParameters WithPartition(
                                                string partition)
        {
            return new ProducerParameters(this, partition: partition);
        }

        #endregion // WithPartition

        #region WithShard

        /// <summary>
        /// Withes the shard.
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <returns></returns>
        internal ProducerParameters WithShard(
                                                string shard)
        {
            return new ProducerParameters(this, shard: shard);
        }

        #endregion // WithShard

        #region AddRoute

        /// <summary>
        /// Adds the route.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns></returns>
        internal ProducerParameters AddRoute(
                                                IProducerHooksBuilder route)
        {
            return new ProducerParameters(this, routes: Routes.Add(route));
        }

        #endregion // AddRoute

        #region AddSegmentation

        /// <summary>
        /// Adds the segmentation.
        /// </summary>
        /// <param name="segmentation">The segmentation.</param>
        /// <returns></returns>
        internal ProducerParameters AddSegmentation(
                                                IProducerAsyncSegmentationStrategy segmentation)
        {
            return new ProducerParameters(this, 
                            segmentationStrategies: SegmentationStrategies.Add(segmentation));
        }

        #endregion // AddSegmentation

        #region AddInterceptor

        /// <summary>
        /// Adds the interceptor.
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns></returns>
        internal ProducerParameters AddInterceptor(
                                  IProducerAsyncInterceptor interceptor)
        {
            return new ProducerParameters(this, 
                            interceptors: Interceptors.Add(interceptor));
        }

        #endregion // AddInterceptor
    }
}
