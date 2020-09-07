using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    public static class RedisProviderExtensions
    {
        private const string PRODUCER_END_POINT_KEY = "REDIS_EVENT_SOURCE_PRODUCER_ENDPOINT";
        private const string PRODUCER_PASSWORD_KEY = "REDIS_EVENT_SOURCE_PRODUCER_PASS";
        private const string CONSUMER_END_POINT_KEY = "REDIS_EVENT_SOURCE_CONSUMER_ENDPOINT";
        private const string CONSUMER_PASSWORD_KEY = "REDIS_EVENT_SOURCE_CONSUMER_PASS";

        public static async ValueTask<IConsumerOptionsBuilder> UseRedisConsumerChannelAsync(
                            this IConsumerBuilder builder,
                            string consumerGroup,
                            ILogger logger,
                            Func<ConfigurationOptions, CancellationToken, ValueTask> configuration,
                            CancellationToken cancellationToken = default,
                            string endpointEnvKey = CONSUMER_END_POINT_KEY,
                            string passwordEnvKey = CONSUMER_PASSWORD_KEY)
        {
            var options = ConfigurationOptionsFactory.FromEnv(endpointEnvKey, passwordEnvKey);
            await configuration(options, cancellationToken);
            var channel = new RedisConsumerChannel(
                                        logger,
                                        options,
                                        endpointEnvKey,
                                        passwordEnvKey);
            cancellationToken.ThrowIfCancellationRequested();
            IConsumerOptionsBuilder result = builder.UseChannel(channel);
            return result;
        }


        #region UseRedisProducerChannelAsync

        /// <summary>
        /// Uses REDIS producer channel.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="endpointEnvKey">The endpoint env key.</param>
        /// <param name="passwordEnvKey">The password env key.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static async ValueTask<IProducerOptionsBuilder> UseRedisProducerChannelAsync(
                            this IProducerBuilder builder,
                            ILogger logger,
                            Func<ConfigurationOptions, CancellationToken, ValueTask> configuration,
                            CancellationToken cancellationToken = default,
                            string endpointEnvKey = PRODUCER_END_POINT_KEY,
                            string passwordEnvKey = PRODUCER_PASSWORD_KEY)
        {
            var options = ConfigurationOptionsFactory.FromEnv(endpointEnvKey, passwordEnvKey);
            await configuration(options, cancellationToken);
            var channel = new RedisProducerChannel(
                                        logger,
                                        options,
                                        endpointEnvKey,
                                        passwordEnvKey);
            cancellationToken.ThrowIfCancellationRequested();
            IProducerOptionsBuilder result = builder.UseChannel(channel);
            return result;
        }

        #endregion // UseRedisProducerChannelAsync

        #region CreateConsumerGroupIfNotExistsAsync

        /// <summary>
        /// Creates the consumer group if not exists asynchronous.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="eventSourceKey">The event source key.</param>
        /// <param name="consumerGroup">The consumer group.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        internal static async Task CreateConsumerGroupIfNotExistsAsync(
                        this IDatabaseAsync db,
                        string eventSourceKey,
                        string consumerGroup,
                        ILogger logger)
        {
            StreamGroupInfo[] groupsInfo = Array.Empty<StreamGroupInfo>();

            int delay = 0;
            bool exists = false;
            int tryNumber = 0;
            while (groupsInfo.Length == 0)
            {
                tryNumber++;
                try
                {
                    #region delay on retry

                    if (delay == 0)
                        delay = 4;
                    else
                    {
                        delay = Math.Min(delay * 2, 15_000);
                        await Task.Delay(delay);
                        if (tryNumber % 10 == 0)
                            logger.LogWarning($"{nameof(CreateConsumerGroupIfNotExistsAsync)} still waiting");
                    }

                    #endregion // delay on retry

                    groupsInfo = await db.StreamGroupInfoAsync(
                                                eventSourceKey,
                                                flags: CommandFlags.DemandMaster);
                    exists = groupsInfo.Any(m => m.Name == consumerGroup);
                }
                #region Exception Handling

                catch (RedisServerException ex)
                {
                    if (!await db.KeyExistsAsync(eventSourceKey,
                                                 flags: CommandFlags.DemandMaster))
                    {
                        logger.LogDebug(ex, $"{nameof(CreateConsumerGroupIfNotExistsAsync)} [GroupInfo]: Key not exists");
                    }
                    else
                    {
                        logger.LogWarning(ex, $"{nameof(CreateConsumerGroupIfNotExistsAsync)} [GroupInfo]: failed");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"{nameof(CreateConsumerGroupIfNotExistsAsync)} [GroupInfo]: unexpected failure");
                }

                #endregion // Exception Handling
                if (!exists)
                {
                    try
                    {
                        await db.StreamCreateConsumerGroupAsync(eventSourceKey,
                                                                consumerGroup,
                                                                StreamPosition.Beginning,
                                                                flags: CommandFlags.DemandMaster);
                    }
                    #region Exception Handling

                    catch (RedisServerException ex)
                    {
                        logger.LogWarning(ex, $"{nameof(CreateConsumerGroupIfNotExistsAsync)}: failed & still waiting");
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, $"{nameof(CreateConsumerGroupIfNotExistsAsync)}: unexpected failure");
                    }

                    #endregion // Exception Handling
                }
            }
        }

        #endregion // CreateConsumerGroupIfNotExistsAsync

    }
}
