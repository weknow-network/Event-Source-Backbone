﻿using System.Collections.Immutable;
using System.Threading.Channels;

namespace EventSourcing.Backbone
{

    /// <summary>
    /// In-Memory Channel (excellent for testing)
    /// </summary>
    /// <seealso cref="EventSourcing.Backbone.IProducerChannelProvider" />
    public class ProducerTestChannel :
                            IProducerChannelProvider
    {
        private readonly Channel<Announcement> _channel;
        private int _index;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="channel">The channel.</param>
        public ProducerTestChannel(Channel<Announcement> channel)
        {
            _channel = channel;
        }

        #endregion // Ctor

        #region SendAsync

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        public async ValueTask<string> SendAsync(
            IProducerPlan plan,
            Announcement payload)
        {
            ImmutableArray<IProducerStorageStrategy> storageStrategy = plan.StorageStrategies;
            foreach (var strategy in storageStrategy)
            {
                await strategy.SaveBucketAsync(payload.Segments, EventBucketCategories.Segments, payload.Metadata);
                await strategy.SaveBucketAsync(payload.InterceptorsData, EventBucketCategories.Interceptions, payload.Metadata);
            }
            await _channel.Writer.WriteAsync(payload);
            return Interlocked.Increment(ref _index).ToString();
        }

        #endregion // SendAsync
    }
}
