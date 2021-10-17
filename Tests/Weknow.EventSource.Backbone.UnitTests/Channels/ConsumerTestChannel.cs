using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{

    public class ConsumerTestChannel : IConsumerChannelProvider
    {
        private readonly Channel<Announcement> _channel;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="channel">The channel.</param>
        public ConsumerTestChannel(Channel<Announcement> channel)
        {
            _channel = channel;
        }

        #endregion // Ctor

        #region SubsribeAsync

        /// <summary>
        /// Subscribe to the channel for specific metadata.
        /// </summary>
        /// <param name="plan">The metadata.</param>
        /// <param name="func">The function.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>When completed</returns>
        public async ValueTask SubsribeAsync(
                    IConsumerPlan plan,
                    Func<Announcement, IAck, ValueTask> func,
                    ConsumerOptions options,
                    CancellationToken cancellationToken)
        {
            while (!_channel.Reader.Completion.IsCompleted &&
                   !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var announcement = await _channel.Reader.ReadAsync(cancellationToken);
                    foreach (var strategy in await plan.StorageStrategiesAsync)
                    {
                        await strategy.LoadBucketAsync(announcement.Metadata, Bucket.Empty, EventBucketCategories.Segments, m => string.Empty);
                        await strategy.LoadBucketAsync(announcement.Metadata, Bucket.Empty, EventBucketCategories.Interceptions, m => string.Empty);
                    }
                    await func(announcement, Ack.Empty);
                }
                catch (ChannelClosedException) { }
            }
        }


        #endregion // SubsribeAsync

        #region GetByIdAsync

        public ValueTask<Announcement> GetByIdAsync(string entryId, IConsumerPlan plan, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion // GetByIdAsync
    }
}
