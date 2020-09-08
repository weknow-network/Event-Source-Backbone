using System;
using System.Threading;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    public delegate Task AckCallbackAsync();

    /// <summary>
    /// Channel provider responsible for passing the actual message 
    /// from producer to consumer. 
    /// </summary>
    public interface IConsumerChannelProvider
    {
        /// <summary>
        /// Subscribe to the channel for specific metadata.
        /// </summary>
        /// <param name="plan">The consumer plan.</param>
        /// <param name="func">The function.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>When completed</returns>
        ValueTask SubsribeAsync(
                    IConsumerPlan plan,
                    Func<Announcement, AckCallbackAsync, ValueTask> func,
                    IEventSourceConsumerOptions options,
                    CancellationToken cancellationToken);
    }
}