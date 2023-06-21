using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace EventSourcing.Backbone.Channels.RedisProvider.Common;


internal class Telemetry
{
    public readonly static Meter Metics = new(RedisChannelConstants.REDIS_CHANNEL_SOURCE);
    public static readonly ActivitySource Track = new ActivitySource(RedisChannelConstants.REDIS_CHANNEL_SOURCE);
}
