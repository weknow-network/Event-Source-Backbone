using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;

using StackExchange.Redis;

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    public static class RedisTelemetryrExtensions
    {
        #region InjectTelemetryTags

        /// <summary>
        /// Adds standard open-telemetry tags (for redis).
        /// </summary>
        /// <param name="meta">The meta.</param>
        /// <param name="activity">The activity.</param>
        public static void InjectTelemetryTags(this Metadata meta, Activity? activity)
        {
            // These tags are added demonstrating the semantic conventions of the OpenTelemetry messaging specification
            // See:
            //   * https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#messaging-attributes
            activity?.SetTag("messaging.system", "redis-stream");
            activity?.SetTag("messaging.destination_kind", "topic");
            activity?.SetTag("messaging.redis.key", $"{meta.Partition}:{meta.Shard}");
        }

        #endregion // InjectTelemetryTags
    }
}
