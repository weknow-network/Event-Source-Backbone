
using System;
using System.Threading;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.Channels;
using Weknow.EventSource.Backbone.Channels.RedisProvider;

namespace Weknow.EventSource.Backbone
{
    internal class RedisConsumerChannelBuilder : IRedisConsumerChannelBuilder
    {
        private readonly IConsumerBuilder _builder;
        private readonly RedisConsumerChannel _channel;
        private readonly IConsumerOptionsBuilder _optionBuilder;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="channel">The channel.</param>
        /// <param name="optionBuilder">The option builder.</param>
        public RedisConsumerChannelBuilder(
            IConsumerBuilder builder,
            RedisConsumerChannel channel,
            IConsumerOptionsBuilder optionBuilder)
        {
            _builder = builder;
            _channel = channel;
            _optionBuilder = optionBuilder;
        }

        #endregion // Ctor

        #region AddStorageStrategy

        /// <summary>
        /// Adds the storage strategy (Segment / Interceptions).
        /// Will use default storage (REDIS Hash) when empty.
        /// When adding more than one it will to all, act as a fall-back (first win, can use for caching).
        /// It important the consumer's storage will be in sync with this setting.
        /// </summary>
        /// <param name="storageStrategy">Storage strategy provider.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        IRedisConsumerChannelBuilder IRedisConsumerChannelBuilder.AddStorageStrategy(
            IConsumerStorageStrategy storageStrategy,
            StorageType targetType)
        {
            var decorated = new FilteredStorageStrategy(storageStrategy, targetType);
            var strategy = _channel.StorageStrategy.Add(decorated);
            var channel = new RedisConsumerChannel(_channel, strategy);
            IConsumerOptionsBuilder optionBuilder = _builder.UseChannel(channel);
            var result = new RedisConsumerChannelBuilder(_builder, channel, optionBuilder);
            return result;
        }

        #endregion // AddStorageStrategy

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
        IConsumerShardBuilder IConsumerPartitionBuilder.Partition(string partition) => _optionBuilder.Partition(partition);

        #endregion // Partition

        #region RegisterInterceptor

        /// <summary>
        /// Register raw interceptor.
        /// Intercept the consumer side execution before de-serialization.
        /// </summary>
        /// <param name="interceptorData">The interceptor data as the interceptor defined in the producer stage.</param>
        /// <returns></returns>
        IConsumerHooksBuilder IConsumerHooksBuilder.RegisterInterceptor(IConsumerInterceptor interceptorData) => _optionBuilder.RegisterInterceptor(interceptorData);

        /// <summary>
        /// Register raw interceptor.
        /// Intercept the consumer side execution before de-serialization.
        /// </summary>
        /// <param name="interceptorData">The interceptor data as the interceptor defined in the producer stage.</param>
        /// <returns></returns>
        IConsumerHooksBuilder IConsumerHooksBuilder.RegisterInterceptor(IConsumerAsyncInterceptor interceptorData) => _optionBuilder.RegisterInterceptor(interceptorData);

        #endregion // RegisterInterceptor

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
        IConsumerHooksBuilder IConsumerHooksBuilder.RegisterSegmentationStrategy(IConsumerSegmentationStrategy segmentationStrategy) => _optionBuilder.RegisterSegmentationStrategy(segmentationStrategy);

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
        IConsumerHooksBuilder IConsumerHooksBuilder.RegisterSegmentationStrategy(IConsumerAsyncSegmentationStrategy segmentationStrategy) => _optionBuilder.RegisterSegmentationStrategy(segmentationStrategy);

        #endregion // RegisterSegmentationStrategy

        #region WithCancellation

        /// <summary>
        /// Withes the cancellation token.
        /// </summary>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        IConsumerHooksBuilder IConsumerHooksBuilder.WithCancellation(CancellationToken cancellation) => _optionBuilder.WithCancellation(cancellation);

        #endregion // WithCancellation

        #region WithOptions

        /// <summary>
        /// Attach configuration.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        IConsumerHooksBuilder IConsumerOptionsBuilder.WithOptions(IEventSourceConsumerOptions options) => _optionBuilder.WithOptions(options);

        #endregion // WithOptions
    }
}
