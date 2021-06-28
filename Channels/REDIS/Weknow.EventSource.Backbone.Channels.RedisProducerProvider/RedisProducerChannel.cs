using Microsoft.Extensions.Logging;

using Polly;

using StackExchange.Redis;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    internal class RedisProducerChannel : IProducerChannelProvider
    {
        private readonly ILogger _logger;
        private readonly AsyncPolicy _resiliencePolicy;
        private static int _index = 0;
        private const string CONNECTION_NAME_PATTERN = "Event_Source_Producer_{0}";
        private readonly Task<IDatabaseAsync> _dbTask;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="resiliencePolicy">The resilience policy for retry.</param>
        /// <param name="endpointEnvKey">The endpoint env key.</param>
        /// <param name="passwordEnvKey">The password env key.</param>
        public RedisProducerChannel(
                        ILogger logger,
                        Action<ConfigurationOptions>? configuration,
                        AsyncPolicy? resiliencePolicy,
                        string endpointEnvKey,
                        string passwordEnvKey)
        {
            _logger = logger;
            _resiliencePolicy = resiliencePolicy ?? 
                                Policy.Handle<Exception>()
                                      .RetryAsync(3); // TODO: [bnaya 2021-02] onRetry -> open telemetry
            string name = string.Format(
                                    CONNECTION_NAME_PATTERN,
                                    Interlocked.Increment(ref _index));
            var redisClientFactory = new RedisClientFactory(
                                                logger,
                                                name,
                                                RedisUsageIntent.Write,
                                                configuration,
                                                endpointEnvKey, passwordEnvKey);
            _dbTask = redisClientFactory.GetDbAsync();

        }

        #endregion // Ctor

        #region SendAsync

        /// <summary>
        /// Sends raw announcement.
        /// </summary>
        /// <param name="payload">The raw announcement data.</param>
        /// <returns>
        /// Return the message id
        /// </returns>
        public async ValueTask<string> SendAsync(Announcement payload)
        {
            IDatabaseAsync db = await _dbTask;

            Metadata meta = payload.Metadata;
            string id = meta.MessageId;

            #region var entries = new NameValueEntry[]{...}

            // local method
            NameValueEntry KV(RedisValue key, RedisValue value) => new NameValueEntry(key, value);
            var entries = new NameValueEntry[] 
            {
                KV(nameof(meta.MessageId), id),
                KV(nameof(meta.Operation), meta.Operation),
                KV(nameof(meta.ProducedAt), meta.ProducedAt.ToUnixTimeSeconds())
            };

            #endregion // var entries = new NameValueEntry[]{...}

            // TODO: [bnaya 2021-01] enable to replace the segments storage without replacing the strweam

            #region await db.HashSetAsync($"Segments~{id}", segmentsEntities)

            var segmentsEntities = payload.Segments
                                            .Select(sgm => 
                                                    new HashEntry(sgm.Key, sgm.Value))
                                            .ToArray();
            await db.HashSetAsync($"Segments~{id}", segmentsEntities);

            #endregion // await db.HashSetAsync($"Segments~{id}", segmentsEntities)

            #region await db.HashSetAsync($"Interceptors~{id}", interceptionsEntities)

            var interceptionsEntities = payload.InterceptorsData
                                            .Select(spt => 
                                                    new HashEntry(spt.Key, spt.Value))
                                            .ToArray();
            await db.HashSetAsync($"Interceptors~{id}", interceptionsEntities);

            #endregion // await db.HashSetAsync($"Interceptors~{id}", interceptionsEntities)

            RedisValue messageId = await _resiliencePolicy.ExecuteAsync(() => 
                                                db.StreamAddAsync(meta.Key(), entries,
                                                   flags: CommandFlags.DemandMaster));


            return messageId;
        }

        #endregion // SendAsync
    }
}
