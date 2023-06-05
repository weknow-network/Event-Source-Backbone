using System.Text.Json;
using System.Text.Json.Serialization;

using EventSourcing.Backbone;

using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using WebSample.Extensions;


// Configuration: https://medium.com/@gparlakov/the-confusion-of-asp-net-configuration-with-environment-variables-c06c545ef732


namespace WebSample.Extensions
{
    /// <summary>
    ///  open telemetry extensions for ASP.NET Core
    /// </summary>
    public static class OpenTelemetryExtensions
    {
        #region AddOpenTelemetry

        /// <summary>
        /// Adds the  open-telemetry binding.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="hostEnv">The host env.</param>
        /// <param name="appName">Short name of the application.</param>
        /// <param name="sampler">An open telemetry sampler.</param>
        /// <param name="telemetryPathFilter">The telemetry path filter.</param>
        /// <returns></returns>
        public static IServiceCollection AddOpenTelemetry(
            this IServiceCollection services,
            IHostEnvironment hostEnv,
            string appName,
            Sampler? sampler = null,
            Func<string?, bool>? telemetryPathFilter = null)
        {
            // see: https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Jaeger/README.md#environment-variables

            Console.WriteLine($"JAEGER endpoint: key='OTEL_EXPORTER_JAEGER_ENDPOINT', env='{hostEnv.EnvironmentName}'"); // will be visible in the pods logs

#pragma warning disable S125 // Sections of code should not be commented out
            services.AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    TracerProviderBuilder tracerProviderBuilder =
                        builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                            .AddService(appName))
                            .ListenToEventSourceRedisChannel()
                            .AddAspNetCoreInstrumentation(m =>
                            {
                                m.Filter = OpenTelemetryFilter;
                                // m.Enrich
                                m.RecordException = true;
                                m.EnableGrpcAspNetCoreSupport = true;
                            })
                            .AddHttpClientInstrumentation(m =>
                            {
                                // m.Enrich
                                m.RecordException = true;
                            })
                            //.AddRedisInstrumentation(redisConnection
                            //        //, m => { 
                            //        //    m.FlushInterval
                            //        //}
                            //        )
                            .AddOtlpExporter();
                    if (sampler == null)
                        tracerProviderBuilder.SetSampler<AlwaysOnSampler>();
                    else
                        tracerProviderBuilder.SetSampler(sampler);

                    if (hostEnv.IsDevelopment())
                    {
                        builder.AddConsoleExporter(options => options.Targets = ConsoleExporterOutputTargets.Console);
                    }
                });
            return services;
#pragma warning restore S125 // Sections of code should not be commented out

            #region OpenTelemetryFilter

            bool OpenTelemetryFilter(HttpContext context)
            {
                string? path = context.Request.Path.Value;
                return telemetryPathFilter?.Invoke(path) ?? OpenTelemetryFilterMap(path);
            }

            bool OpenTelemetryFilterMap(string? path)
            {
                if (string.IsNullOrEmpty(path) ||
                    path == "/health" ||
                    path == "/readiness" ||
                    path == "/version" ||
                    path == "/settings" ||
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

        #endregion // AddOpenTelemetry

        #region WithJsonOptions

        /// <summary>
        /// Set Controller's with the standard json configuration.
        /// </summary>
        /// <param name="controllers">The controllers.</param>
        /// <returns></returns>
        public static IMvcBuilder WithJsonOptions(
            this IMvcBuilder controllers)
        {
            return controllers.AddJsonOptions(options =>
            {
                // https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to
                JsonSerializerOptions setting = options.JsonSerializerOptions;
                setting.WithDefault();

            });
        }

        #endregion // WithJsonOptions

        #region WithDefault

        public static JsonSerializerOptions WithDefault(this JsonSerializerOptions options)
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            options.WriteIndented = true;
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            options.Converters.Add(JsonMemoryBytesConverterFactory.Default);
            return options;
        }

        #endregion // WithDefault
    }
}
