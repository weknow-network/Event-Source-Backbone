using System.Collections.Immutable;

namespace EventSourcing.Backbone
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
        /// <param name="plan">The builder's plan.</param>
        /// <param name="payload">The raw announcement data.</param>
        /// <returns>
        /// Return the message id
        /// </returns>
        ValueTask<string> SendAsync(
            IProducerPlan plan,
            Announcement payload);
    }
}