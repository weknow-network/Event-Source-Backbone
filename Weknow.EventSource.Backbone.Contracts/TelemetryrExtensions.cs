using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Weknow.EventSource.Backbone
{
    public static class TelemetryrExtensions
    {
        #region InjectMetaTelemetryTags

        /// <summary>
        /// Adds standard open-telemetry tags (for redis).
        /// </summary>
        /// <param name="meta">The meta.</param>
        /// <param name="activity">The activity.</param>
        public static void InjectMetaTelemetryTags(this Metadata meta, Activity? activity)
        {
            activity?.SetTag("event-source.partition", meta.Partition);
            activity?.SetTag("event-source.shard", meta.Shard);
            activity?.SetTag("event-source.operation", meta.Operation);
            activity?.SetTag("event-source.message-id", meta.MessageId);
            activity?.SetTag("event-source.channel-type", meta.ChannelType);
        }

        #endregion // InjectMetaTelemetryTags
    }
}
