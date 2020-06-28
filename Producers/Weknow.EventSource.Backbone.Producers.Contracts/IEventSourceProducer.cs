using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Event Source producer.
    /// </summary>
    /// <typeparam name="T">type of the segment's data.</typeparam>
    public interface IEventSourceProducer<T>
        where T: notnull
    {
        /// <summary>
        /// Posts a message to the given channel.
        /// </summary>
        /// <param name="segmentData">The message to publish into specific segment.</param>
        /// <param name="overrideIntent">
        /// When not null will override the default intent (as set in the builder)
        /// </param>
        /// <returns></returns>
        Task SendAsync(
            T segmentData,
            string? overrideIntent = null);
    }
}
