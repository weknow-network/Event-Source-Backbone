using EventSourcing.Backbone;

using Microsoft.Extensions.DependencyInjection;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// see:
//  https://opentelemetry.io/docs/instrumentation/net/getting-started/
//  https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Jaeger/README.md#environment-variables

namespace ConsoleTest;

/// <summary>
/// Open telemetry extensions for ASP.NET Core
/// </summary>
internal static class OpenTelemetryExtensions
{
    #region AddOpenTelemetryEventSourcing

    /// <summary>
    /// Adds open telemetry for event sourcing.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="environment">The environment.</param>
    /// <returns></returns>
    public static IServiceCollection AddOpenTelemetryEventSourcing(this IServiceCollection services, Env environment)
    {
        // see:
        //  https://opentelemetry.io/docs/instrumentation/net/getting-started/
        //  https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Jaeger/README.md#environment-variables
        services.AddOpenTelemetry()
                .WithEventSourcingTracing(environment,
                        cfg =>
                        {
                            cfg.SetResourceBuilder(ResourceBuilder.CreateDefault()
                                .AddService("ConsoleTest"))
                            .AddOtlpExporter();
                            // .AddConsoleExporter();
                        })
                .WithEventSourcingMetrics(environment, cfg =>
                {
                    cfg
                        .AddOtlpExporter()
                        .AddPrometheusExporter()
                        .AddMeter("ConsoleTest");
                });

        return services;
    }

    #endregion // AddOpenTelemetryEventSourcing
}
