using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Channel provider responsible for passing the actual message 
    /// from producer to consumer. 
    /// </summary>
    public interface IProducerChannelProvider
    {

        /// <summary>
        /// Sends raw announcement.
        /// </summary>
        /// <param name="payload">The raw announcement data.</param>
        /// <param name="storageStrategy">The storage strategy.</param>
        /// <returns>
        /// Return the message id
        /// </returns>
        ValueTask<string> SendAsync(
            Announcement payload,
            ImmutableArray<IProducerStorageStrategyWithFilter> storageStrategy);
    }
}