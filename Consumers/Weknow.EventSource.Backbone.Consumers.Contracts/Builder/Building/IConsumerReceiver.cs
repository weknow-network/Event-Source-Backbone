using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Receive data (on demand data query).
    /// </summary>
    public interface IConsumerReceiver: IConsumerBuilderEnvironment<IConsumerReceiver>
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
