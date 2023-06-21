using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace EventSourcing.Backbone.Channels.RedisProvider;

internal class Telemetry
{
    public readonly static Meter Metrics = new(ProducerChannelConstants.REDIS_CHANNEL_SOURCE);
    public static readonly ActivitySource Track = new ActivitySource(ProducerChannelConstants.REDIS_CHANNEL_SOURCE);
}
