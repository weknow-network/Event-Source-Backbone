﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using Weknow.EventSource.Backbone.Channels.RedisProvider;

namespace Weknow.EventSource.Backbone.Channels
{
    /// <summary>
    /// Responsible to save information to REDIS hash storage.
    /// The information can be either Segmentation or Interception.
    /// When adding it via the builder it can be arrange in a chain in order of having
    /// 'Chain of Responsibility' for saving different parts into different storage (For example GDPR's PII).
    /// Alternative, chain can serve as a cache layer.
    /// </summary>
    internal class RedisHashStorageStrategy : IProducerStorageStrategy
    {
        private readonly IEventSourceRedisConnectionFacroty _connFactory;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="connFactory">The connection factory.</param>
        /// <param name="logger">The logger.</param>
        public RedisHashStorageStrategy(
                        IEventSourceRedisConnectionFacroty connFactory,
                        ILogger logger)
        {
            _connFactory = connFactory;
            _logger = logger;
        }

        /// <summary>
        /// Saves the bucket information.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="bucket">Either Segments or Interceptions.</param>
        /// <param name="type">The type.</param>
        /// <param name="meta">The metadata.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>
        /// Array of metadata entries which can be used by the consumer side storage strategy, in order to fetch the data.
        /// </returns>
        async ValueTask<IImmutableDictionary<string, string>> IProducerStorageStrategy.SaveBucketAsync(
                                                                    string id,
                                                                    Bucket bucket,
                                                                    EventBucketCategories type,
                                                                    Metadata meta,
                                                                    CancellationToken cancellation)
        {
            try
            {
                var conn = await _connFactory.GetAsync();
                IDatabaseAsync db = conn.GetDatabase();

                var segmentsEntities = bucket
                                                .Select(sgm =>
                                                        new HashEntry(sgm.Key, sgm.Value))
                                                .ToArray();
                await db.HashSetAsync($"{type}~{id}", segmentsEntities);
                return ImmutableDictionary<string, string>.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fail to Save event's [{id}] buckets [{type}], into the [{partition}->{shard}] stream: {operation}",
                    id, type, meta.Partition, meta.Shard, meta.Operation);
                throw;
            }
        }

    }
}
