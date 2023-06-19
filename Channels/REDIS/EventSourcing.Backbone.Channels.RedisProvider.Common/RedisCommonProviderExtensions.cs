using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace EventSourcing.Backbone.Private
{
    /// <summary>
    /// Redis common provider extensions
    /// </summary>
    public static class RedisCommonProviderExtensions
    {
        private const int MAX_DELAY = 15_000;
        private const int KEY_NOT_EXISTS_DELAY = 3_000;

        private static readonly AsyncLock _lock = new AsyncLock(TimeSpan.FromSeconds(20));

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
                        this IEventSourceRedisConnectionFactory connFactory,
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

                IConnectionMultiplexer conn = await connFactory.GetAsync();
                IDatabaseAsync db = conn.GetDatabase();
                try
                {
                    #region Validation (if key exists)

                    if (!await db.KeyExistsAsync(eventSourceKey,
                                                 flags: CommandFlags.DemandMaster))
                    {
                        await Task.Delay(KEY_NOT_EXISTS_DELAY);
                        logger.LogDebug("Key not exists (yet): {info}", CurrentInfo());
                        continue;
                    }

                    #endregion // Validation (if key exists)

                    #region delay on retry

                    if (delay == 0)
                        delay = 4;
                    else
                    {
                        delay = Math.Min(delay * 2, MAX_DELAY);
                        await Task.Delay(delay);
                        if (tryNumber % 10 == 0)
                        {
                            logger.LogWarning("Create Consumer Group If Not Exists: still waiting {info}", CurrentInfo());
                        }
                    }


                    #endregion // delay on retry

                    using var lk = await _lock.AcquireAsync();
                    groupsInfo = await db.StreamGroupInfoAsync(
                                                eventSourceKey,
                                                flags: CommandFlags.DemandMaster);
                    exists = groupsInfo.Any(m => m.Name == consumerGroup);
                }
                #region Exception Handling

                catch (RedisServerException ex)
                {
                    if (await db.KeyExistsAsync(eventSourceKey,
                                                 flags: CommandFlags.DemandMaster))
                    {
                        logger.LogWarning(ex, "Create Consumer Group If Not Exists: failed. {info}", CurrentInfo());
                    }
                    else
                    {
                        await Task.Delay(KEY_NOT_EXISTS_DELAY);
                        logger.LogDebug(ex, "Create Consumer Group If Not Exists: failed. {info}", CurrentInfo());
                    }
                }
                catch (RedisConnectionException ex)
                {
                    logger.LogWarning(ex.FormatLazy(), "Create Consumer Group If Not Exists: connection failure. {info}", CurrentInfo());
                }
                catch (RedisTimeoutException ex)
                {
                    logger.LogWarning(ex.FormatLazy(), "Create Consumer Group If Not Exists:  timeout failure. {info}", CurrentInfo());
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex.FormatLazy(), "Create Consumer Group If Not Exists:  unexpected failure. {info}", CurrentInfo());
                }

                #endregion // Exception Handling
                if (!exists)
                {
                    try
                    {
                        using var lk = await _lock.AcquireAsync();
                        if (await db.StreamCreateConsumerGroupAsync(eventSourceKey,
                                                                consumerGroup,
                                                                StreamPosition.Beginning,
                                                                flags: CommandFlags.DemandMaster))
                        {
                            break;
                        }
                    }
                    #region Exception Handling

                    catch (RedisServerException ex)
                    {
                        logger.LogWarning(ex.FormatLazy(), $"""
                                                {nameof(CreateConsumerGroupIfNotExistsAsync)}.StreamCreateConsumerGroupAsync: 
                                                failed & still waiting 
                                                {CurrentInfo()}
                                                """);
                    }
                    catch (RedisConnectionException ex)
                    {
                        logger.LogWarning(ex.FormatLazy(), $"""
                                                {nameof(CreateConsumerGroupIfNotExistsAsync)}.StreamCreateConsumerGroupAsync: 
                                                Connection failure 
                                                {CurrentInfo()}
                                                """);
                    }
                    catch (ObjectDisposedException)
                    {
                        logger.LogWarning($"""
                                                {nameof(CreateConsumerGroupIfNotExistsAsync)}.StreamCreateConsumerGroupAsync: 
                                                Connection might not being available 
                                                {CurrentInfo()}
                                                """);
                    }

                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, $"""
                                                {nameof(CreateConsumerGroupIfNotExistsAsync)}.StreamCreateConsumerGroupAsync: 
                                                unexpected failure 
                                                {CurrentInfo()}
                                                """);
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
