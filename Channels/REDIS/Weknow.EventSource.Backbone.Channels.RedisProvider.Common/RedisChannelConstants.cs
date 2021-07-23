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

        public const string CONSUMER_END_POINT_KEY = "REDIS_EVENT_SOURCE_CONSUMER_ENDPOINT";
        public const string CONSUMER_PASSWORD_KEY = "REDIS_EVENT_SOURCE_CONSUMER_PASS";


        public static class MetaKeys
        {
            public const string SegmentsKeys = "segments-keys";
            public const string InterceptorsKeys = "interceptors-keys";
        }
    }
}
