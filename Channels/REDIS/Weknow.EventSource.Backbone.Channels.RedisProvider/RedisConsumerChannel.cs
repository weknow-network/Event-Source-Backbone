using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    public class RedisConsumerChannel : IConsumerChannelProvider
    {
        private readonly ILogger _logger;
        private readonly ConfigurationOptions _options;
        private static int _index;
        private const string CONNECTION_NAME_PATTERN = "Event_Source_Consumer_{0}";
        private readonly RedisClientFactory _redisClientFactory;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RedisConsumerChannel(
            ILogger logger,
            ConfigurationOptions options,
            string endpointEnvKey,
            string passwordEnvKey)
        {
            _logger = logger;
            _options = options;
            string name = string.Format(
                                    CONNECTION_NAME_PATTERN,
                                    Interlocked.Increment(ref _index));
            _redisClientFactory = new RedisClientFactory(
                                                logger,
                                                name,
                                                RedisUsageIntent.Write,
                                                endpointEnvKey, passwordEnvKey);
            //_dbTask = _redisClientFactory.GetDbAsync();
            //redisClientFactory.GetSubscriberAsync()
        }

        #endregion // Ctor

        /// <summary>
        /// Subscribe to the channel for specific metadata.
        /// </summary>
        /// <param name="plan">The consumer plan.</param>
        /// <param name="func">The function.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>When completed</returns>
        public async ValueTask SubsribeAsync(
                    IConsumerPlan plan,
                    Func<Announcement, ValueTask> func,
                    IEventSourceConsumerOptions options,
                    CancellationToken cancellationToken)
        {
            string key = $"{plan.Partition}:{plan.Shard}";

            IDatabaseAsync db = await _redisClientFactory.GetDbAsync();
            await db.CreateConsumerGroupIfNotExistsAsync(
                key,
                plan.ConsumerGroup,
                plan.Logger ?? _logger);


            // TODO: loop
            StreamEntry[] values = await db.StreamReadGroupAsync(
                                                    key, 
                                                    plan.ConsumerGroup, 
                                                    plan.ConsumerName,
                                                    position: StreamPosition.Beginning,
                                                    count: options.BatchSize);
        }
    }
}
