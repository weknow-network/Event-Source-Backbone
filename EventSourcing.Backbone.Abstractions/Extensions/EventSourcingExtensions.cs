using System.Diagnostics;

using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;

using static System.Diagnostics.TelemetryrExtensions;
namespace EventSourcing.Backbone;

public static class EventSourcingExtensions
{
    /// <summary>
    /// Adds the event consumer telemetry source (will result in tracing the consumer).
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    public static TracerProviderBuilder ListenToEventSourceRedisChannel(
                                                this TracerProviderBuilder builder) =>
                                                        builder.AddSource(
                                                            EventSourceConstants.REDIS_CONSUMER_CHANNEL_SOURCE,
                                                            EventSourceConstants.REDIS_PRODUCER_CHANNEL_SOURCE);

    #region ExtractSpan

    /// <summary>
    /// Extract telemetry span's parent info
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="meta">The meta.</param>
    /// <param name="entries">The entries for extraction.</param>
    /// <param name="injectStrategy">The injection strategy.</param>
    /// <returns></returns>
    public static ActivityContext ExtractSpan<T>(
                    this Metadata meta,
                    T entries,
                    Func<T, string, IEnumerable<string>> injectStrategy)
    {
        PropagationContext parentContext = Propagator.Extract(default, entries, injectStrategy);
        Baggage.Current = parentContext.Baggage;

        // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
        // https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#span-name
        return parentContext.ActivityContext;
    }

    #endregion // ExtractSpan

    #region InjectMetaTelemetryTags

    /// <summary>
    /// Adds standard open-telemetry tags (for redis).
    /// </summary>
    /// <param name="meta">The meta.</param>
    /// <param name="activity">The activity.</param>
    public static void InjectMetaTelemetryTags(this Metadata meta, Activity? activity)
    {
        activity?.SetTag("event-source.uri", meta.Uri);
        activity?.SetTag("event-source.operation", meta.Operation);
        activity?.SetTag("event-source.message-id", meta.MessageId);
        activity?.SetTag("event-source.channel-type", meta.ChannelType);
    }

    #endregion // InjectMetaTelemetryTags
}
