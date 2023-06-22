using EventSourcing.Backbone;
using EventSourcing.Backbone.Channels;
using EventSourcing.Backbone.Channels.RedisProvider.Common;

using Microsoft.Extensions.Hosting;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


// Configuration: https://medium.com/@gparlakov/the-confusion-of-asp-net-configuration-with-environment-variables-c06c545ef732
// see:
//  https://opentelemetry.io/docs/instrumentation/net/getting-started/
//  https://opentelemetry.io/docs/demo/services/cart/
//  https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Jaeger/README.md#environment-variables
//  https://opentelemetry.io/docs/demo/docker-deployment/

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// core extensions for ASP.NET Core
/// </summary>
public static class EventSourcingOtel
{
    #region WithEventSourcingTracing

    #region Overloads

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

    #endregion // Overloads

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
                                           ProducerChannelConstants.REDIS_CHANNEL_SOURCE,
                                           ConsumerChannelConstants.REDIS_CHANNEL_SOURCE,
                                           RedisChannelConstants.REDIS_CHANNEL_SOURCE,
                                           EventSourceConstants.TELEMETRY_SOURCE };

                    tracerProviderBuilder
                        .AddSource(sources)
                        .ConfigureResource(resource => resource.AddService(env));

                    injection?.Invoke(tracerProviderBuilder);
                });

        return builder;
    }

    #endregion // WithEventSourcingTracing

    #region WithEventSourcingMetrics

    #region Overloads

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

    #endregion // Overloads

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
                    injection?.Invoke(metricsProviderBuilder);
                    metricsProviderBuilder
                        //.ConfigureResource(resource => resource.AddService(env)
                        //                                                    .AddService(ConsumerChannelConstants.REDIS_CHANNEL_SOURCE))
                        .AddMeter(env,
                                ProducerChannelConstants.REDIS_CHANNEL_SOURCE,
                                ConsumerChannelConstants.REDIS_CHANNEL_SOURCE,
                                RedisChannelConstants.REDIS_CHANNEL_SOURCE,
                                EventSourceConstants.TELEMETRY_SOURCE);
                });

        return builder;
    }

    #endregion // WithEventSourcingMetrics
}
