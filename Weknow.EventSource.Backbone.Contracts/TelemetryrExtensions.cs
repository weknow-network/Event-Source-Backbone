using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Weknow.EventSource.Backbone
{
    public static class TelemetryrExtensions
    {
        /// <summary>
        /// Adds the event consumer telemetry source (will result in tracing the consumer).
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static TracerProviderBuilder ListenToEventSourceRedisChannel(this TracerProviderBuilder builder) =>
                                                                builder.AddSource(
                                                                    EventSourceConstants.REDIS_CONSUMER_CHANNEL_SOURCE,
                                                                    EventSourceConstants.REDIS_PRODUCER_CHANNEL_SOURCE);

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
