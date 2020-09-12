using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Private;

using static System.Math;

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    internal class RedisConsumerChannel : IConsumerChannelProvider
    {
        private const int MAX_DELAY = 1500;

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

        #region SubsribeAsync

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
        public async ValueTask SubsribeAsync(
                    IConsumerPlan plan,
                    Func<Announcement, IAck, ValueTask> func,
                    IEventSourceConsumerOptions options,
                    CancellationToken cancellationToken)
        {
            string key = $"{plan.Partition}:{plan.Shard}";
            CommandFlags flags = plan.Options.AckBehavior == AckBehavior.FireAndForget ?
                                      CommandFlags.FireAndForget :
                                      CommandFlags.None;

            IDatabaseAsync db = await _redisClientFactory.GetDbAsync();
            await db.CreateConsumerGroupIfNotExistsAsync(
                key,
                plan.ConsumerGroup,
                plan.Logger ?? _logger);

            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(
                                        cancellationToken, plan.Cancellation).Token;
            int delay = 1;
            while (!cancellationToken.IsCancellationRequested)
            {
                StreamEntry[] results = await ReadBatchAsync();

                await DelayIfEmpty(results.Length);

                foreach (StreamEntry result in results)
                {
                    var entries = result.Values.ToDictionary(m => m.Name, m => m.Value);
                    string id = entries[nameof(Metadata.Empty.MessageId)];
                    string segmentsKey = $"Segments~{id}";
                    string interceptorsKey = $"Interceptors~{id}";

                    string operation = entries[nameof(Metadata.Empty.Operation)];
                    long producedAtUnix = (long)entries[nameof(Metadata.Empty.ProducedAt)];
                    DateTimeOffset producedAt = DateTimeOffset.FromUnixTimeSeconds(producedAtUnix);
                    var meta = new Metadata(id, plan.Partition, plan.Shard, operation, producedAt);

                    var segmentsEntities = await db.HashGetAllAsync(segmentsKey, CommandFlags.DemandMaster); // DemandMaster avoid racing
                    var segmentsPairs = segmentsEntities.Select(m => ((string)m.Name, (byte[])m.Value));
                    var interceptionsEntities = await db.HashGetAllAsync(interceptorsKey, CommandFlags.DemandMaster); // DemandMaster avoid racing
                    var interceptionsPairs = interceptionsEntities.Select(m => ((string)m.Name, (byte[])m.Value));

                    var segmets = Bucket.Empty.AddRange(segmentsPairs);
                    var interceptions = Bucket.Empty.AddRange(interceptionsPairs);

                    var announcement = new Announcement(meta, segmets, interceptions);
                    var ack = new AckOnce(() => AckAsync(result.Id), plan.Options.AckBehavior);
                    await func(announcement, ack);
                }
            }

            #region ReadBatchAsync

            async Task<StreamEntry[]> ReadBatchAsync()
            {
                // TBD: circuit-breaker
                try
                {
                    StreamEntry[] values = await db.StreamReadGroupAsync(
                                                        key,
                                                        plan.ConsumerGroup,
                                                        plan.ConsumerName,
                                                        position: StreamPosition.NewMessages,
                                                        count: options.BatchSize,
                                                        flags: flags);
                    return values;

                }
                catch (RedisTimeoutException ex)
                {
                    _logger.LogWarning(ex, "Event source [{source}] by [{consumer}]: Timeout", key, plan.ConsumerName);
                    return Array.Empty<StreamEntry>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fail to read from event source [{source}] by [{consumer}]", key, plan.ConsumerName);
                    return Array.Empty<StreamEntry>();
                }
            }

            #endregion // ReadBatchAsync

            #region AckAsync

            async ValueTask AckAsync(RedisValue messageId)
            {
                // release the event (won't handle again in the future)
                long id = await db.StreamAcknowledgeAsync(key,
                                                plan.ConsumerGroup, 
                                                messageId,
                                                flags: CommandFlags.DemandMaster);
            }

            #endregion // AckAsync

            #region DelayIfEmpty

            async Task<int> DelayIfEmpty(int resultsLength)
            {
                if (resultsLength == 0)
                {
                    await Task.Delay(delay, cancellationToken);
                    delay *= Max(delay, 2);
                    delay = Min(MAX_DELAY, delay);
                }
                else
                    delay = 1;
                return delay;
            }

            #endregion // DelayIfEmpty
        }

        #endregion // SubsribeAsync
    }
}
