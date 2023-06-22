using System.Diagnostics;
using System.Diagnostics.Metrics;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using static EventSourcing.Backbone.Channels.RedisProvider.Common.Telemetry;

namespace EventSourcing.Backbone.Private
{
    /// <summary>
    /// Redis common provider extensions
    /// </summary>
    public static class RedisCommonProviderExtensions
    {
        private const int DELAY_ON_MISSING_KEY = 5;
        private const int MIN_DELAY = 2;
        private const int SPIN_LIMIT = 30;
        private const int MAX_DELAY = 3_000;
        private static Counter<int> KeyMissingCounter = Metics.CreateCounter<int>("event-source.key-missing", "count", "count missing key events");
        private static Counter<int> CreateConsumerGroupCounter = Metics.CreateCounter<int>("event-source.create-consumer-group", "count", "creating a consumer group");
        private static Counter<int> CreateConsumerGroupRetryCounter = Metics.CreateCounter<int>("event-source.create-consumer-group-retry", "count", "retries of creating a consumer group");

        private static readonly AsyncLock _lock = new AsyncLock(TimeSpan.FromSeconds(20));

        #region CreateConsumerGroupIfNotExistsAsync

        /// <summary>
        /// Creates the consumer group if not exists asynchronous.
        /// </summary>
        /// <param name="connFactory">The connection factory.</param>
        /// <param name="uri">The event source URI.</param>
        /// <param name="consumerGroup">The consumer group.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task CreateConsumerGroupIfNotExistsAsync(
                        this IEventSourceRedisConnectionFactory connFactory,
                        string uri,
                        string consumerGroup,
                        ILogger logger,
                        CancellationToken cancellationToken)
        {
            StreamGroupInfo[] groupsInfo = Array.Empty<StreamGroupInfo>();
            using var track = Track.StartInternalTrace("event-source.consumer.create-consumer-group", t => t.Add("group-name", consumerGroup));
            CreateConsumerGroupCounter.WithTag("URI", uri).WithTag("consumer-group", consumerGroup).Add(1);
            int delay = MIN_DELAY;
            bool exists = false;
            int tryNumber = 0;
            var retryCounter = CreateConsumerGroupRetryCounter.WithTag("URI", uri).WithTag("consumer-group", consumerGroup);
            var missingCounter = KeyMissingCounter.WithTag("URI", uri).WithTag("consumer-group", consumerGroup);
            while (groupsInfo.Length == 0)
            {
                if (tryNumber != 0)
                    retryCounter.Add(1);
                tryNumber++;

                IConnectionMultiplexer conn = await connFactory.GetAsync(cancellationToken);
                IDatabaseAsync db = conn.GetDatabase();
                try
                {
                    #region delay on retry

                    if (tryNumber > SPIN_LIMIT)
                    {
                        delay = Math.Min(delay * 2, MAX_DELAY);
                        using (Track.StartInternalTrace("event-source.consumer.delay.key-not-exists",
                                            t => t.Add("delay", delay)
                                                            .Add("try-number", tryNumber)
                                                            .Add("group-name", consumerGroup))) ;
                        {
                            await Task.Delay(delay);
                        }
                        if (tryNumber % 10 == 0)
                        {
                            logger.LogWarning("Create Consumer Group If Not Exists: still waiting {info}", CurrentInfo());
                        }
                    }

                    #endregion // delay on retry

                    #region Validation (if key exists)

                    if (!await db.KeyExistsAsync(uri,
                                                 flags: CommandFlags.DemandMaster))
                    {
                        missingCounter.Add(1);
                        await Task.Delay(DELAY_ON_MISSING_KEY);
                        if (tryNumber == 0 || tryNumber > SPIN_LIMIT)
                            logger.LogDebug("Key not exists (yet): {info}", CurrentInfo());
                        continue;
                    }

                    #endregion // Validation (if key exists)

                    using (Track.StartInternalTrace("event-source.consumer.get-consumer-group-info",
                                        t => t
                                                        .Add("try-number", tryNumber)
                                                        .Add("group-name", consumerGroup))) ;
                    {
                        using var lk = await _lock.AcquireAsync(cancellationToken);
                        groupsInfo = await db.StreamGroupInfoAsync(
                                                    uri,
                                                    flags: CommandFlags.DemandMaster);
                        exists = groupsInfo.Any(m => m.Name == consumerGroup);
                    }
                }
                #region Exception Handling

                catch (RedisServerException ex)
                {
                    if (await db.KeyExistsAsync(uri,
                                                 flags: CommandFlags.DemandMaster))
                    {
                        logger.LogWarning(ex, "Create Consumer Group If Not Exists: failed. {info}", CurrentInfo());
                    }
                    else
                    {
                        //await Task.Delay(KEY_NOT_EXISTS_DELAY);
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
                        using (Track.StartInternalTrace("event-source.consumer.create-consumer-group",
                                            t => t
                                                            .Add("try-number", tryNumber)
                                                            .Add("group-name", consumerGroup))) ;
                        {
                            using var lk = await _lock.AcquireAsync(cancellationToken);
                            if (await db.StreamCreateConsumerGroupAsync(uri,
                                                                    consumerGroup,
                                                                    StreamPosition.Beginning,
                                                                    flags: CommandFlags.DemandMaster))
                            {
                                break;
                            }
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
Stream key:     {uri}
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
