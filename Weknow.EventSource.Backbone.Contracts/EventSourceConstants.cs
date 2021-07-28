using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Constants
    /// </summary>
    public static class EventSourceConstants
    {
        /// <summary>
        /// The name of redis consumer channel source
        /// </summary>
        public const string REDIS_CONSUMER_CHANNEL_SOURCE = "redis-consumer-channel";
        /// <summary>
        /// The name of redis producer channel source
        /// </summary>
        public const string REDIS_PRODUCER_CHANNEL_SOURCE = "redis-producer-channel";
    }
}
