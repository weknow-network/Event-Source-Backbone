using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace EventSourcing.Backbone.Private;

public class EventSourceTelemetry
{
    public readonly static Meter EMeter = new(EventSourceConstants.TELEMETRY_SOURCE);
    public static readonly ActivitySource ETracer = new ActivitySource(EventSourceConstants.TELEMETRY_SOURCE);
}
