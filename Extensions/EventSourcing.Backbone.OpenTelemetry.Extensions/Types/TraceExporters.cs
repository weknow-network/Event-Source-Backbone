namespace EventSourcing.Backbone;

/// <summary>
/// Tracing exporter options (open-telemetry)
/// </summary>
[Flags]
public enum TraceExporters
{
    None = 0,
    Otlp =  1,
    DevConsole = Otlp * 2,
    Default = Otlp | DevConsole
}