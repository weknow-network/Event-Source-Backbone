using System;
using System.Linq;
using System.Threading;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.CodeGeneration;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Event Source consumer builder.
    /// </summary>
    public class ConsumerBuilder :
        IConsumerBuilder,
        IConsumerOptionsBuilder,
        IConsumerShardBuilder
    {
        private readonly ConsumerParameters _parameters = ConsumerParameters.Empty;

        /// <summary>
        /// Event Source consumer builder.
        /// </summary>
        public static readonly IConsumerBuilder Empty = new ConsumerBuilder();

        #region Ctor

        /// <summary>
        /// Prevents a default instance of the <see cref="ConsumerBuilder"/> class from being created.
        /// </summary>
        private ConsumerBuilder()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        private ConsumerBuilder(ConsumerParameters parameters)
        {
            _parameters = parameters;
        }

        #endregion // Ctor

        #region UseChannel

        /// <summary>
        /// Choose the communication channel provider.
        /// </summary>
        /// <param name="channel">The channel provider.</param>
        /// <returns></returns>
        IConsumerOptionsBuilder IConsumerBuilder.UseChannel(
                        IConsumerChannelProvider channel)
        {
            var prms = _parameters.WithChannel(channel);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        #endregion // UseChannel

        #region WithOptions

        /// <summary>
        /// Attach configuration.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        IConsumerHooksBuilder IConsumerOptionsBuilder.WithOptions(IEventSourceConsumerOptions options)
        {
            var prms = _parameters.WithOptions(options);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        #endregion // WithOptions

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
        IConsumerShardBuilder IConsumerPartitionBuilder.Partition(
                                    string partition)
        {
            var prms = _parameters.WithPartition(partition);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        #endregion // Partition

        #region Shard

        /// <summary>
        /// Shard key represent physical sequence.
        /// On the consumer side shard is optional
        /// for listening on a physical source rather on the entire partition.
        /// Use same shard when order is matter.
        /// For example: assuming each ORDERING flow can have its
        /// own messaging sequence, in this case you can split each
        /// ORDER into different shard and gain performance bust..
        /// </summary>
        /// <param name="shard">The shard key.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IConsumerSubscribeBuilder Shard(string shard)
        {
            var prms = _parameters.WithShard(shard);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        #endregion // Shard

        #region RegisterSegmentationStrategy

        /// <summary>
        /// Responsible of building instance from segmented data.
        /// Segmented data is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="segmentationStrategy">The segmentation strategy.</param>
        /// <returns></returns>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        IConsumerHooksBuilder IConsumerHooksBuilder.RegisterSegmentationStrategy(IConsumerSegmentationStrategy segmentationStrategy)
        {
            var bridge = new ConsumerSegmentationStrategyBridge(segmentationStrategy);
            var prms = _parameters.AddSegmentation(bridge);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        /// <summary>
        /// Responsible of building instance from segmented data.
        /// Segmented data is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="segmentationStrategy">The segmentation strategy.</param>
        /// <returns></returns>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        IConsumerHooksBuilder IConsumerHooksBuilder.RegisterSegmentationStrategy(IConsumerAsyncSegmentationStrategy segmentationStrategy)
        {
            var prms = _parameters.AddSegmentation(segmentationStrategy);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        #endregion // RegisterSegmentationStrategy

        #region RegisterInterceptor

        /// <summary>
        /// Registers the interceptor.
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns></returns>
        IConsumerHooksBuilder IConsumerHooksBuilder.RegisterInterceptor(
                            IConsumerInterceptor interceptor)
        {
            var bridge = new ConsumerInterceptorBridge(interceptor);
            var prms = _parameters.AddInterceptor(bridge);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        /// <summary>
        /// Registers the interceptor.
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns></returns>
        IConsumerHooksBuilder IConsumerHooksBuilder.RegisterInterceptor(
                            IConsumerAsyncInterceptor interceptor)
        {
            var prms = _parameters.AddInterceptor(interceptor);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        #endregion // RegisterInterceptor

        #region Subscribe

        /// <summary>
        /// Subscribe consumer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factory">The factory.</param>
        /// <returns>
        /// The partition subscription (dispose to remove the subscription)
        /// </returns>
        IAsyncDisposable IConsumerSubscribeBuilder.Subscribe<T>(
            Func<ConsumerMetadata, T> factory)

        {
            #region Validation

            if (_parameters == null)
                throw new ArgumentNullException(nameof(_parameters));

            #endregion // Validation

            var parameters = _parameters;
            if (parameters.SegmentationStrategies.Count == 0)
                parameters = parameters.AddSegmentation(new ConsumerDefaultSegmentationStrategy());

            var consumer = new ConsumerBase<T>(parameters, factory);
            var subscription = consumer.Subscribe();
            return subscription;
        }

        #endregion // Subscribe

        #region WithCancellation

        /// <summary>
        /// Withes the cancellation token.
        /// </summary>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        IConsumerHooksBuilder IConsumerCancellationBuilder<IConsumerHooksBuilder>.WithCancellation(CancellationToken cancellation)
        {
            var prms = _parameters.WithCancellation(cancellation);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        /// <summary>
        /// Withes the cancellation token.
        /// </summary>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        IConsumerOptionsBuilder IConsumerCancellationBuilder<IConsumerOptionsBuilder>.WithCancellation(CancellationToken cancellation)
        {
            var prms = _parameters.WithCancellation(cancellation);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        #endregion // WithCancellation
    }
}
