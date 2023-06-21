using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace EventSourcing.Backbone.Channels.RedisProvider;

internal class Telemetry
{
    public readonly static Meter Metrics = new(ConsumerChannelConstants.REDIS_CHANNEL_SOURCE);
    public static readonly ActivitySource Track = new ActivitySource(ConsumerChannelConstants.REDIS_CHANNEL_SOURCE);

    //private static readonly Counter<int> DelayCounter = Metics.CreateCounter<int>("event-source.consumer.delay", "",
    //                                        "");

}
