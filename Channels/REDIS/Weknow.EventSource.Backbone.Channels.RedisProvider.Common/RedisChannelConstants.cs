using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.Channels.RedisProvider.Common
{
    public static class RedisChannelConstants
    {
        public const string CHANNEL_TYPE = "REDIS Channel V1";
        public const string META_ARRAY_SEPARATOR = "~|~";

        public const string END_POINT_KEY = "REDIS_EVENT_STREAM_ENDPOINT";
        public const string PASSWORD_KEY = "REDIS_EVENT_STREAM_PASS";


        public static class MetaKeys
        {
            public const string SegmentsKeys = "segments-keys";
            public const string InterceptorsKeys = "interceptors-keys";
            public const string TelemetryBaggage = "telemetry-baggage";
            public const string TelemetrySpanId = "telemetry-span-id";
            public const string TelemetryTraceId = "telemetry-trace-id";
        }
    }
}
