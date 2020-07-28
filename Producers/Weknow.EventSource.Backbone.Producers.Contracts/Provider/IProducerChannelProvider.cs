using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

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
        ValueTask SendAsync(Announcement payload);
    }
}