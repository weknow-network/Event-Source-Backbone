using StackExchange.Redis;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System.Reflection;

namespace EventSourcing.Backbone;

/// <summary>
/// REDIS client factory
/// </summary>
public static class RedisExtensions
{
    private static readonly AssemblyName ASSEMBLY = Assembly.GetEntryAssembly().GetName();
    public static IConnectionMultiplexer WithTelemetry(this IConnectionMultiplexer connection)
    {
        string enable = Environment.GetEnvironmentVariable("EVENT_SOURCE_WITH_REDIS_TRACE") ?? "false";
        if (bool.TryParse(enable, out var enableValue) && enableValue)
        {
            Sdk.CreateTracerProviderBuilder()
                            .AddRedisInstrumentation(
                            connection,
                            opt =>
                            {
                                opt.SetVerboseDatabaseStatements = false;
                                opt.FlushInterval = TimeSpan.FromSeconds(5);
                                opt.Enrich = (activity, command) =>
                                                {
                                                    activity.SetTag("app.version", ASSEMBLY.Version);
                                                    activity.SetTag("app.name", ASSEMBLY.Name);
                                                };
                            })
                            .AddOtlpExporter()
                            .Build();
        }
        return connection;
    }
}
