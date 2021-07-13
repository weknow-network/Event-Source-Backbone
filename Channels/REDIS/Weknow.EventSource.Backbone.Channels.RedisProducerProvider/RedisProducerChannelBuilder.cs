
using System;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.Channels;
using Weknow.EventSource.Backbone.Channels.RedisProvider;

namespace Weknow.EventSource.Backbone
{
    internal class RedisProducerChannelBuilder : IRedisProducerChannelBuilder
    {
        private readonly IProducerBuilder _builder;
        private readonly RedisProducerChannel _channel;
        private readonly IProducerOptionsBuilder _optionBuilder;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="channel">The channel.</param>
        /// <param name="optionBuilder">The option builder.</param>
        public RedisProducerChannelBuilder(
            IProducerBuilder builder,
            RedisProducerChannel channel,
            IProducerOptionsBuilder optionBuilder)
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
        /// <param name="filter">The filter of which keys in the bucket will be store into this storage.</param>
        /// <returns></returns>
        IRedisProducerChannelBuilder IRedisProducerChannelBuilder.AddStorageStrategy(
            IProducerStorageStrategy storageStrategy,
            StorageType targetType,
            Predicate<string>? filter)
        {
            var decorated = new FilteredStorageStrategy(storageStrategy, filter, targetType);
            var strategy = _channel.StorageStrategy.Add(decorated);
            var channel = new RedisProducerChannel(_channel, strategy);
            IProducerOptionsBuilder optionBuilder = _builder.UseChannel(channel);
            var result = new RedisProducerChannelBuilder(_builder, channel, optionBuilder);
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
        IProducerShardBuilder IProducerPartitionBuilder.Partition(string partition) => _optionBuilder.Partition(partition);

        #endregion // Partition

        #region WithOptions

        /// <summary>
        /// Apply configuration.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        IProducerPartitionBuilder IProducerOptionsBuilder.WithOptions(IEventSourceOptions options) => _optionBuilder.WithOptions(options);

        #endregion // WithOptions
    }
}
