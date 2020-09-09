using Microsoft.Extensions.Logging;

using System;
using System.Diagnostics;
using System.Threading;

using Weknow.EventSource.Backbone.Building;

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
        private readonly ConsumerPlan _plan = ConsumerPlan.Empty;

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
        /// <param name="plan">The plan.</param>
        private ConsumerBuilder(ConsumerPlan plan)
        {
            _plan = plan;
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
            var prms = _plan.WithChannel(channel);
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
            var prms = _plan.WithOptions(options);
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
            var prms = _plan.WithPartition(partition);
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
            var prms = _plan.WithShard(shard);
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
            var prms = _plan.AddSegmentation(bridge);
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
            var prms = _plan.AddSegmentation(segmentationStrategy);
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
            var prms = _plan.AddInterceptor(bridge);
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
            var prms = _plan.AddInterceptor(interceptor);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        #endregion // RegisterInterceptor

        #region WithLogger

        /// <summary>
        /// Attach logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        IConsumerSubscribeBuilder IConsumerLoggerBuilder.WithLogger(ILogger logger)
        {
            var prms = _plan.WithLogger(logger);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        #endregion // WithLogger

        #region Subscribe

        /// <summary>
        /// Subscribe consumer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factory">The factory.</param>
        /// <param name="consumerGroup">
        /// Consumer Group allow a group of clients to cooperate
        /// consuming a different portion of the same stream of messages
        /// </param>
        /// <param name="consumerName">
        /// Optional Name of the consumer.
        /// Can use for observability.
        /// </param>
        /// <returns>
        /// The partition subscription (dispose to remove the subscription)
        /// </returns>
        IAsyncDisposable IConsumerSubscribeBuilder.Subscribe<T>(
            Func<ConsumerMetadata, T> factory,
            string? consumerGroup,
            string? consumerName)

        {
            #region Validation

            if (_plan == null)
                throw new ArgumentNullException(nameof(_plan));

            #endregion // Validation

            consumerGroup = consumerGroup ?? $"{DateTime.UtcNow:yyyy-MM-dd HH_mm} {Guid.NewGuid():N}";

            ConsumerPlan plan = _plan.WithConsumerGroup(consumerGroup, consumerName);
            if (plan.SegmentationStrategies.Count == 0)
                plan = plan.AddSegmentation(new ConsumerDefaultSegmentationStrategy());

            var consumer = new ConsumerBase<T>(plan, factory);
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
        IConsumerHooksBuilder IConsumerHooksBuilder.WithCancellation(CancellationToken cancellation)
        {
            var prms = _plan.WithCancellation(cancellation);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        #endregion // WithCancellation
    }
}
