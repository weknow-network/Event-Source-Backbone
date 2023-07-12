using System.Threading.Channels;

using Microsoft.Extensions.Logging;

namespace EventSourcing.Backbone
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// When completed
        /// </returns>
        public async ValueTask SubscribeAsync(
                    IConsumerPlan plan,
                    Func<Announcement, IAck, ValueTask<bool>> func,
                    CancellationToken cancellationToken)
        {
            while (!_channel.Reader.Completion.IsCompleted &&
                   !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var announcement = await _channel.Reader.ReadAsync(cancellationToken);
                    foreach (var strategy in plan.StorageStrategies)
                    {
                        await strategy.LoadBucketAsync(announcement.Metadata, Bucket.Empty, EventBucketCategories.Segments, m => string.Empty);
                        await strategy.LoadBucketAsync(announcement.Metadata, Bucket.Empty, EventBucketCategories.Interceptions, m => string.Empty);
                    }
                    await func(announcement, Ack.Empty);
                }
                catch (ChannelClosedException)
                {
                    plan.Logger.LogWarning("Channel closed: {uri}", plan.FullUri());
                }
            }
        }


        #endregion // SubsribeAsync

        #region GetByIdAsync

        public ValueTask<Announcement> GetByIdAsync(EventKey entryId, IConsumerPlan plan, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion // GetByIdAsync

        IAsyncEnumerable<Announcement> IConsumerChannelProvider.GetAsyncEnumerable(IConsumerPlan plan, ConsumerAsyncEnumerableOptions options, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
