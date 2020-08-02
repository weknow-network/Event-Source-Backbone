using System;
using System.Threading;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Channel provider responsible for passing the actual message 
    /// from producer to consumer. 
    /// </summary>
    public interface IConsumerChannelProvider
    {
        /// <summary>
        /// Receives the asynchronous.
        /// </summary>
        /// <param name="func">The function.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        ValueTask ReceiveAsync(
                    Func<Announcement, ValueTask> func,
                    IEventSourceConsumerOptions options,
                    CancellationToken cancellationToken);
    }
}