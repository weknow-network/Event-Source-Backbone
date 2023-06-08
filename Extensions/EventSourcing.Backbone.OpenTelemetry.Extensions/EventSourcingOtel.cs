using System.Diagnostics;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


// Configuration: https://medium.com/@gparlakov/the-confusion-of-asp-net-configuration-with-environment-variables-c06c545ef732


namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///  core extensions for ASP.NET Core
/// </summary>
public static class EventSourcingOtel
{
    /// <summary>
    /// The name of redis consumer channel source
    /// </summary>
    public const string REDIS_CONSUMER_CHANNEL_SOURCE = "redis-consumer-channel";
    /// <summary>
    /// The name of redis producer channel source
    /// </summary>
    public const string REDIS_PRODUCER_CHANNEL_SOURCE = "redis-producer-channel";

    /// <summary>
    /// Adds the event consumer telemetry source (will result in tracing the consumer).
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    private static TracerProviderBuilder ListenToEventSourceRedisChannel(
                                                this TracerProviderBuilder builder) =>
                                                        builder.AddSource(
                                                            REDIS_CONSUMER_CHANNEL_SOURCE,
                                                            REDIS_PRODUCER_CHANNEL_SOURCE);

    /// <summary>
    /// Adds the  open-telemetry binding.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="hostEnv">The host env.</param>
    /// <param name="filter">The filter.</param>
    /// <param name="sampler">The sampler.</param>
    /// <param name="additionalSources">The additional list of subscribe sources for the telemetry.</param>
    /// <returns></returns>
    public static IServiceCollection AddOpenTelemetryForEventSourcing(
        this IServiceCollection services,
        IHostEnvironment hostEnv,
        Func<HttpContext, bool>? filter = null,
        Sampler? sampler = null,
        params string[] additionalSources)
    {
        // see:
        //  https://opentelemetry.io/docs/instrumentation/net/getting-started/
        //  https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Jaeger/README.md#environment-variables

        Func<HttpContext, bool> filtering = filter ?? (Func<HttpContext, bool>)OpenTelemetryFilter;

        var appName = hostEnv.ApplicationName;

#pragma warning disable S125 // Sections of code should not be commented out
        services.AddOpenTelemetry()
                .WithTracing(tracerProviderBuilder =>
                {
                    var sources = new[] { appName,
                                           REDIS_CONSUMER_CHANNEL_SOURCE,
                                           REDIS_PRODUCER_CHANNEL_SOURCE};
                    if(additionalSources != null && additionalSources.Length != 0) 
                    {
                        sources = sources.Concat(additionalSources).ToArray();
                    }

                    tracerProviderBuilder
                        .AddSource(sources)
                        .ConfigureResource(resource => resource.AddService(appName))
                        .AddAspNetCoreInstrumentation(m =>
                        {
                            m.Filter = filtering;
                            // m.Enrich
                            m.RecordException = true;
                            m.EnableGrpcAspNetCoreSupport = true;
                        })
                        .AddHttpClientInstrumentation(m =>
                        {
                            // m.Enrich
                            m.RecordException = true;
                        });
                    if (sampler != null)
                        tracerProviderBuilder.SetSampler(sampler);
                    tracerProviderBuilder.AddOtlpExporter();
                    if (hostEnv.IsDevelopment())
                    {
                        tracerProviderBuilder.AddConsoleExporter(options =>
                                                    options.Targets = ConsoleExporterOutputTargets.Console);
                    }
                })
                .WithMetrics(metricsProviderBuilder =>
                {
                    metricsProviderBuilder
                        .ConfigureResource(resource => resource.AddService(appName))
                        .AddMeter(appName)
                        .AddAspNetCoreInstrumentation(
                        //m => {
                        //    m.Filter = (_, ctx) => filtering(ctx);
                        //}
                        )
                        .AddOtlpExporter();
                    if (hostEnv.IsDevelopment())
                        metricsProviderBuilder.AddConsoleExporter();
                });

        return services;
#pragma warning restore S125 // Sections of code should not be commented out

        #region OpenTelemetryFilter

        bool OpenTelemetryFilter(HttpContext context) => OpenTelemetryFilterMap(context.Request.Path.Value);

        bool OpenTelemetryFilterMap(string? path)
        {
            if (string.IsNullOrEmpty(path) ||
                path == "/health" ||
                path == "/readiness" ||
                path == "/version" ||
                path == "/settings" ||
                path.StartsWith("/v1/kv/") || // configuration 
                path == "/api/v2/write" || // influx metrics
                path == "/_bulk" ||
                path.StartsWith("/swagger") ||
                path.IndexOf("health-check") != -1)
            {
                return false;
            }
            return true;
        }

        #endregion // OpenTelemetryFilter
    }
}
