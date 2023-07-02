using Microsoft.Extensions.Logging;

namespace EventSourcing.Backbone;

/// <summary>
/// Telemetry level
/// </summary>
public record TelemetryLevel
{
    public static readonly TelemetryLevel Default = new TelemetryLevel();
    /// <summary>
    /// Gets the trace level.
    /// </summary>
    public LogLevel Trace { get; init; } = LogLevel.Information;
    /// <summary>
    /// Gets the metric level.
    /// </summary>
    public LogLevel Metric { get; init; } = LogLevel.Information;
}