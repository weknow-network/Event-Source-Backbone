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
    public record ConsumerAsyncEnumerableJsonOptions : ConsumerAsyncEnumerableOptions

    {
        /// <summary>
        /// Ignore metadata.
        /// </summary>
        public bool IgnoreMetadata { get; init; } 
    }
}
