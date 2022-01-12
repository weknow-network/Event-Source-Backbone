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
    public record ConsumerAsyncEnumerableOptions  

    {
        /// <summary>
        /// From key (can be Unix date for Redis provider).
        /// </summary>
        public string? From { get; init; }
        /// <summary>
        /// To key (can be Unix date for Redis provider).
        /// </summary>
        public string? To { get; init; }
        /// <summary>
        /// Operation filter for reduce the results.
        /// </summary>
        public Predicate<Metadata>? OperationFilter { get; init; }
        /// <summary>
        /// Indicating whether to keep looking for future events when the stream is empty (default = true).
        /// </summary>
        /// <value>
        ///   <c>true</c> if [quit when empty]; otherwise, <c>false</c>.
        /// </value>
        public bool ExitWhenEmpty { get; init; } = true;
    }
}
