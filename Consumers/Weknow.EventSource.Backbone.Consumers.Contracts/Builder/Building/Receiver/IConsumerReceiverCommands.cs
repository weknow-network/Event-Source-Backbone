using System.Text.Json;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Receive data (on demand data query).
    /// </summary>
    public interface IConsumerReceiverCommands
    {
        /// <summary>
        /// Gets data by id.
        /// </summary>
        /// <param name="entryId">The entry identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        ValueTask<AnnouncementData> GetByIdAsync(
                                    EventKey entryId,
                                    CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets data by id.
        /// </summary>
        /// <param name="entryId">The entry identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        ValueTask<JsonElement> GetJsonByIdAsync(
                                    EventKey entryId,
                                    CancellationToken cancellationToken = default);
    }
}
