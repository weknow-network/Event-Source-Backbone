using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Server.Kestrel.Core;

using Newtonsoft.Json.Serialization;

using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using StackExchange.Redis;

using EventSource.Backbone;

// Configuration: https://medium.com/@gparlakov/the-confusion-of-asp-net-configuration-with-environment-variables-c06c545ef732


namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Weknow core extensions for ASP.NET Core
    /// </summary>
    public static class AspCoreExtensions
    {
        private static readonly NamingStrategy _namingStrategy = new CamelCaseNamingStrategy();

        #region AddRedis

        /// <summary>
        /// Adds the weknow standard configuration.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="hostEnv">The host env.</param>
        /// <param name="shortAppName"></param>
        /// <returns></returns>
        public static IConnectionMultiplexer AddRedis(
            this IServiceCollection services,
            IHostEnvironment hostEnv,
            string shortAppName)
        {
            IConnectionMultiplexer redisConnection = RedisClientFactory.CreateProviderBlocking();
            services.AddSingleton<IConnectionMultiplexer>(redisConnection);

            return redisConnection;
        }

        #endregion // AddRedis

        #region AddOpenTelemetryWeknow

        /// <summary>
        /// Adds the weknow open-telemetry binding.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="hostEnv">The host env.</param>
        /// <param name="shortAppName">Short name of the application.</param>
        /// <param name="redisConnection">The redis connection.</param>
        /// <returns></returns>
        public static IServiceCollection AddOpenTelemetryWeknow(
            this IServiceCollection services,
            IHostEnvironment hostEnv,
            string shortAppName,
            IConnectionMultiplexer redisConnection)
        {
            // see: https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Jaeger/README.md#environment-variables

            Console.WriteLine($"JAEGER endpoint: key='OTEL_EXPORTER_JAEGER_ENDPOINT', env='{hostEnv.EnvironmentName}'"); // will be visible in the pods logs

#pragma warning disable S125 // Sections of code should not be commented out
            services.AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(shortAppName))
                        .ListenToEventSourceRedisChannel()
                        // .SetSampler<AlwaysOnSampler>()
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
                        .AddOtlpExporter()
                        .SetSampler(TestSampler.Create(LogLevel.Information));
                    
                    if (hostEnv.IsDevelopment())
                    {
                        builder.AddConsoleExporter(options => options.Targets = ConsoleExporterOutputTargets.Console);
                    }
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

        #endregion // AddOpenTelemetryWeknow

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

        public static JsonSerializerOptions WithDefault(this JsonSerializerOptions options)
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            options.WriteIndented = true;
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            options.Converters.Add(JsonMemoryBytesConverterFactory.Default);
            return options;
        }

        #region UseRestDefaultsWeknow

        /// <summary>
        /// Pre-configured host defaults for rest API.
        /// </summary>
        /// <typeparam name="TStartup">The type of the startup.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="args">The process arguments.</param>
        /// <returns></returns>
        public static IHostBuilder UseRestDefaults<TStartup>(
        this IHostBuilder builder,
        params string[] args) where TStartup : class
        {
            builder.ConfigureHostConfiguration(cfg =>
                {
                    // https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.1&tabs=windows
                    // cfg.AddEnvironmentVariables()
                    // cfg.AddJsonFile()
                    // cfg.AddInMemoryCollection()
                    // cfg.AddConfiguration()
                    // cfg.AddUserSecrets()
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(
                        (WebHostBuilderContext context,
                         KestrelServerOptions options) =>
                    {
                        options.ConfigureEndpointDefaults(c => c.Protocols = HttpProtocols.Http1AndHttp2);
                    });
                    webBuilder.UseStartup<TStartup>();
                }).ConfigureLogging((context, builder) =>
                    {
                        builder.AddOpenTelemetry(options =>
                        {
                            options.IncludeScopes = true;
                            options.ParseStateValues = true;
                            options.IncludeFormattedMessage = true;
                        });
                    });
            return builder;
        }

        #endregion // UseRestDefaultsWeknow
    }
}
