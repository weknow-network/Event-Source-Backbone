namespace EventSourcing.Backbone;

/// <summary>
/// Metrics exporter options (open-telemetry)
/// </summary>
[Flags]
public enum MetricsExporters
{
    None = 0,
    Otlp = 1,
    DevConsole = Otlp * 2,
    Prometheus = DevConsole * 2,
    Default = Otlp | Prometheus
}