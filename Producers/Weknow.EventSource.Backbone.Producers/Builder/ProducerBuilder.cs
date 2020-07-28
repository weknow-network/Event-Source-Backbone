using System;
using System.Collections;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.CodeGeneration;

using Segments = System.Collections.Immutable.ImmutableDictionary<string, System.ReadOnlyMemory<byte>>;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public class ProducerBuilder :
        IProducerBuilder,
        IProducerOptionsBuilder,
        IProducerShardBuilder,
        IProducerHooksBuilder
    {
        private readonly ProducerParameters _parameters =
            ProducerParameters.Empty;

        /// <summary>
        /// Event Source producer builder.
        /// </summary>
        public static readonly IProducerBuilder Empty = new ProducerBuilder();

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        private ProducerBuilder()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        internal ProducerBuilder(ProducerParameters parameters)
        {
            _parameters = parameters;
        }

        #endregion // Ctor

        #region Merge

        /// <summary>
        /// Merges multiple channels of same contract into single
        /// producer for broadcasting messages via all channels.
        /// </summary>
        /// <param name="first">The first channel.</param>
        /// <param name="second">The second channel.</param>
        /// <param name="others">The others channels.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IProducerHooksBuilder IProducerBuilder.Merge(
                            IProducerHooksBuilder first,
                            IProducerHooksBuilder second,
                            params IProducerHooksBuilder[] others)
        {
            throw new NotImplementedException();
        }

        #endregion // Merge

        #region UseChannel

        /// <summary>
        /// Choose the communication channel provider.
        /// </summary>
        /// <param name="channel">The channel provider.</param>
        /// <returns></returns>
        IProducerOptionsBuilder IProducerBuilder.UseChannel(
                                                IProducerChannelProvider channel)
        {
            var prms = _parameters.WithChannel(channel);
            return new ProducerBuilder(prms);
        }

        #endregion // UseChannel

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
        /// <param name="partition">The partition key.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IProducerShardBuilder IProducerPartitionBuilder.Partition(string partition)
        {
            var prms = _parameters.WithPartition(partition);
            return new ProducerBuilder(prms);
        }

        #endregion // Partition

        #region Shard

        /// <summary>
        /// Shard key represent physical sequence.
        /// Use same shard when order is matter.
        /// For example: assuming each ORDERING flow can have its
        /// own messaging sequence, in this case you can split each
        /// ORDER into different shard and gain performance bust..
        /// </summary>
        /// <param name="shard">The shard key.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IProducerHooksBuilder IProducerShardBuilder.Shard(string shard)
        {
            var prms = _parameters.WithShard(shard);
            return new ProducerBuilder(prms);
        }

        #endregion // Shard

        #region WithOptions

        /// <summary>
        /// Apply configuration.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IProducerPartitionBuilder IProducerOptionsBuilder.WithOptions(IEventSourceOptions options)
        {
            var prms = _parameters.WithOptions(options);
            return new ProducerBuilder(prms);
        }

        #endregion // WithOptions

        #region UseSegmentation

        /// <summary>
        /// Register segmentation strategy,
        /// Segmentation responsible of splitting an instance into segments.
        /// Segments is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="segmentationStrategy">A strategy of segmentation.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        IProducerHooksBuilder IProducerHooksBuilder.UseSegmentation(
                            IProducerAsyncSegmentationStrategy segmentationStrategy)
        {
            var prms = _parameters.AddSegmentation(segmentationStrategy);
            return new ProducerBuilder(prms);
        }

        /// <summary>
        /// Register segmentation strategy,
        /// Segmentation responsible of splitting an instance into segments.
        /// Segments is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="segmentationStrategy">A strategy of segmentation.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        IProducerHooksBuilder IProducerHooksBuilder.UseSegmentation(IProducerSegmentationStrategy segmentationStrategy)
        {
            var asyncImp = new ProducerSegmentationStrategyBridge(segmentationStrategy);
            var prms = _parameters.AddSegmentation(asyncImp);
            return new ProducerBuilder(prms);
        }

        #endregion // UseSegmentation

        #region AddInterceptor

        /// <summary>
        /// Adds Producer interceptor (stage = after serialization).
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IProducerHooksBuilder IProducerHooksBuilder.AddInterceptor(
                                                IProducerInterceptor interceptor)
        {
            var bridge = new ProducerInterceptorBridge(interceptor);
            var prms = _parameters.AddInterceptor(bridge);
            return new ProducerBuilder(prms);
        }

        /// <summary>
        /// Adds Producer interceptor (Timing: after serialization).
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IProducerHooksBuilder IProducerHooksBuilder.AddInterceptor(
            IProducerAsyncInterceptor interceptor)
        {
            var prms = _parameters.AddInterceptor(interceptor);
            return new ProducerBuilder(prms);
        }

        #endregion // AddInterceptor

        #region Build

        /// <summary>
        /// <![CDATA[ Ceate Producer proxy for specific events sequence.
        /// Event sequence define by an interface which declare the
        /// operations which in time will be serialized into event's
        /// messages.
        /// This interface can be use as a proxy in the producer side,
        /// and as adapter on the consumer side.
        /// All method of the interface should represent one-way communication pattern
        /// and return Task or ValueTask (not Task<T> or ValueTask<T>).
        /// Nothing but method allowed on this interface]]>
        /// </summary>
        /// <typeparam name="T">The contract of the proxy / adapter</typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        T IProducerSpecializeBuilder.Build<T>()
        {
            return new CodeGenerator("DynamicProxies").CreateProducerProxy<T, ProducerBase>(_parameters.Channel, _parameters.Options.Serializer);
        }

        #endregion // Build
    }

    public class ProducerBase
    {
        private IProducerChannelProvider _channel;
        protected IDataSerializer _serializer;

        public ProducerBase(IProducerChannelProvider channel, IDataSerializer serializer)
        {
            _channel = channel;
            _serializer = serializer;
        }

        protected async ValueTask SendAsync(Segments segments)
        {
            var announcment = new Announcement(new Metadata(Guid.NewGuid().ToString(), DateTime.Now), segments);
            await _channel.SendAsync(announcment);
        }
    }
}
