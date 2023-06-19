using EventSourcing.Backbone;

using Microsoft.Extensions.Hosting;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


// Configuration: https://medium.com/@gparlakov/the-confusion-of-asp-net-configuration-with-environment-variables-c06c545ef732
// see:
//  https://opentelemetry.io/docs/instrumentation/net/getting-started/
//  https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Jaeger/README.md#environment-variables



namespace Microsoft.Extensions.DependencyInjection;


/// <summary>
/// core extensions for ASP.NET Core
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

    #region WithEventSourcingTracing

    /// <summary>
    /// Adds the  open-telemetry tracing binding.
    /// </summary>
    /// <param name="builder">The build.</param>
    /// <param name="hostEnv">The host env.</param>
    /// <param name="injection">Enable to inject additional setting.</param>
    /// <returns></returns>
    public static OpenTelemetryBuilder WithEventSourcingTracing(
        this OpenTelemetryBuilder builder,
        IHostEnvironment hostEnv,
        Action<TracerProviderBuilder>? injection = null)
    {
        var env = hostEnv.ApplicationName;
        return builder.WithEventSourcingTracing(env, injection);
    }

    /// <summary>
    /// Adds the  open-telemetry tracing binding.
    /// </summary>
    /// <param name="builder">The build.</param>
    /// <param name="env">The environment.</param>
    /// <param name="injection">Enable to inject additional setting.</param>
    /// <returns></returns>
    public static OpenTelemetryBuilder WithEventSourcingTracing(
        this OpenTelemetryBuilder builder,
        Env env,
        Action<TracerProviderBuilder>? injection = null)
    {
        builder
                .WithTracing(tracerProviderBuilder =>
                {
                    var sources = new[] { (string)env,
                                           REDIS_CONSUMER_CHANNEL_SOURCE,
                                           REDIS_PRODUCER_CHANNEL_SOURCE};

                    tracerProviderBuilder
                        .AddSource(sources)
                        .ConfigureResource(resource => resource.AddService(env));

                    injection?.Invoke(tracerProviderBuilder);
                });

        return builder;
    }

    #endregion // WithEventSourcingTracing

    #region WithEventSourcingMetrics

    /// <summary>
    /// Adds the  open-telemetry metrics binding.
    /// </summary>
    /// <param name="builder">The build.</param>
    /// <param name="hostEnv">The host env.</param>
    /// <param name="injection">The injection.</param>
    /// <returns></returns>
    public static OpenTelemetryBuilder WithEventSourcingMetrics(
        this OpenTelemetryBuilder builder,
        IHostEnvironment hostEnv,
        Action<MeterProviderBuilder>? injection = null)
    {
        var env = hostEnv.ApplicationName;
        return builder.WithEventSourcingMetrics(env, injection);
    }

    /// <summary>
    /// Adds the  open-telemetry metrics binding.
    /// </summary>
    /// <param name="builder">The build.</param>
    /// <param name="env">The host environment.</param>
    /// <param name="injection">The injection.</param>
    /// <returns></returns>
    public static OpenTelemetryBuilder WithEventSourcingMetrics(
        this OpenTelemetryBuilder builder,
        Env env,
        Action<MeterProviderBuilder>? injection = null)
    {
        builder.WithMetrics(metricsProviderBuilder =>
                {
                    metricsProviderBuilder
                        .ConfigureResource(resource => resource.AddService(env));
                    injection?.Invoke(metricsProviderBuilder);
                });

        return builder;
    }

    #endregion // WithEventSourcingMetrics
}
