using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Receive data (on demand data query).
    /// </summary>
    public interface IConsumerReceiver
    {
        /// <summary>
        /// Gets data by id.
        /// </summary>
        /// <param name="entryId">The entry identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        ValueTask<AnnouncementData> GetByIdAsync(
                                    EventKey entryId,
                                    CancellationToken cancellationToken);
    }
}
