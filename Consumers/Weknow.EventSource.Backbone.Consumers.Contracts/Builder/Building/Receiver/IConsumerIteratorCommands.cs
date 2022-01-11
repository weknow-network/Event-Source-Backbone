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
    public interface IConsumerIteratorCommands
    {
        /// <summary>
        /// Gets asynchronous enumerable of announcements.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        IAsyncEnumerable<Announcement> GetAsyncEnumerable(
            ConsumerAsyncEnumerableOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets asynchronous enumerable of announcements.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        IAsyncEnumerable<JsonElement> GetJsonAsyncEnumerable(
            ConsumerAsyncEnumerableOptions? options = null,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Receive data (on demand data query).
    /// </summary>
    public interface IConsumerIteratorCommands<TEntityFamily>
    {

        /// <summary>
        /// Gets asynchronous enumerable of announcements.
        /// </summary>
        /// <typeparam name="TCast">This type is used for filtering the result, only result of this type will yield.</typeparam>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        IAsyncEnumerable<TCast> GetAsyncEnumerable<TCast>(
            ConsumerAsyncEnumerableOptions? options = null,
            CancellationToken cancellationToken = default)
            where TCast : TEntityFamily;
    }
}
