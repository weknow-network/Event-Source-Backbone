using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.Private
{
    public static class RedisCommonProviderExtensions
    {
        #region CreateConsumerGroupIfNotExistsAsync

        /// <summary>
        /// Creates the consumer group if not exists asynchronous.
        /// </summary>
        /// <param name="connFactory">The connection factory.</param>
        /// <param name="eventSourceKey">The event source key.</param>
        /// <param name="consumerGroup">The consumer group.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        public static async Task CreateConsumerGroupIfNotExistsAsync(
                        this IEventSourceRedisConnectionFacroty connFactory,
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

                IConnectionMultiplexer conn = await connFactory.CreateAsync();
                IDatabaseAsync db = conn.GetDatabase();
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
                            logger.LogWarning($"{nameof(CreateConsumerGroupIfNotExistsAsync)} still waiting {CurrentInfo()}");
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
                        logger.LogDebug(ex, $"{nameof(CreateConsumerGroupIfNotExistsAsync)}.KeyExistsAsync [GroupInfo]: Key not exists");
                    }
                    else
                    {
                        logger.LogWarning(ex, $"{nameof(CreateConsumerGroupIfNotExistsAsync)}.KeyExistsAsync [GroupInfo]: failed");
                    }
                }
                catch (RedisConnectionException ex)
                {
                    logger.LogWarning(ex.FormatLazy(), $"{nameof(CreateConsumerGroupIfNotExistsAsync)}.KeyExistsAsync: connection failure {CurrentInfo()}");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"{nameof(CreateConsumerGroupIfNotExistsAsync)}.KeyExistsAsync [GroupInfo]: unexpected failure");
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
                        logger.LogWarning(ex.FormatLazy(), $"{nameof(CreateConsumerGroupIfNotExistsAsync)}.StreamCreateConsumerGroupAsync: failed & still waiting {CurrentInfo()}");
                    }
                    catch (RedisConnectionException ex)
                    {
                        logger.LogWarning(ex.FormatLazy(), $"{nameof(CreateConsumerGroupIfNotExistsAsync)}.StreamCreateConsumerGroupAsync: connection failure {CurrentInfo()}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex.FormatLazy(), $"{nameof(CreateConsumerGroupIfNotExistsAsync)}.StreamCreateConsumerGroupAsync: unexpected failure {CurrentInfo()}");
                    }

                    #endregion // Exception Handling
                }

                #region string CurrentInfo()

                string CurrentInfo() => @$"
Try number:     {tryNumber}
Stream key:     {eventSourceKey}
Consumer Group: {consumerGroup}
Is Connected:   {db.Multiplexer.IsConnected}
Configuration:  {db.Multiplexer.Configuration}
";

                #endregion // string CurrentInfo()
            }
        }

        #endregion // CreateConsumerGroupIfNotExistsAsync
    }
}
