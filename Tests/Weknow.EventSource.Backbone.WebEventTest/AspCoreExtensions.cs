using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Threading.Tasks;
using System.Linq;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Text.Json;
using Microsoft.Extensions.Logging.Console;
using StackExchange.Redis;
using Weknow.EventSource.Backbone;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using NJsonSchema.Generation;
using System.Text.Json.Serialization;

// Configuration: https://medium.com/@gparlakov/the-confusion-of-asp-net-configuration-with-environment-variables-c06c545ef732


namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Weknow core extensions for ASP.NET Core
    /// </summary>
    public static class AspCoreExtensions
    {
        private const string REDIS_KEY = "REDIS";
        private const string REDIS_DEFAULT = "localhost:6379";
        /* Jaeger: 
         * Local:
         *  - Run Docker: docker run --name jaeger --rm -it -p16686:16686 -p 55680:55680 jaegertracing/opentelemetry-all-in-one
         *  - Browse: http://localhost:16686/search      
         * Learn more: https://www.jaegertracing.io/docs/1.21/opentelemetry/
         */
        private const string JAEGER_ENDPOINT = "JAEGER_ENDPOINT_OTPL";
        private const string JAEGER_DEFAULT = "http://localhost:55680";
        private static readonly NamingStrategy _namingStrategy = new CamelCaseNamingStrategy();


        #region CofigureStandardWeknow

        /// <summary>
        /// Configure weknow standard.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="args">The process arguments.</param>
        /// <returns></returns>
        private static IHostBuilder CofigureStandardWeknow(
            this IHostBuilder host,
            params string[] args)
        {
            #region ConfigureHostConfiguration

            host = host.ConfigureHostConfiguration(cfg =>
                   {
                       // https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.1&tabs=windows
                       // cfg.AddEnvironmentVariables()
                       // cfg.AddJsonFile()
                       // cfg.AddInMemoryCollection()
                       // cfg.AddConfiguration()
                       // cfg.AddUserSecrets()
                   });

            #endregion // ConfigureHostConfiguration

            return host;
        }

        #endregion // CofigureStandardWeknow

        #region AddStandardWeknow

        /// <summary>
        /// Adds the weknow standard configuration.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="hostEnv">The host env.</param>
        /// <returns></returns>
        public static IServiceCollection AddStandardWeknow(
            this IServiceCollection services,
            IHostEnvironment hostEnv)
        {
            string env = hostEnv.EnvironmentName;
            string appName = hostEnv.ApplicationName;
            string shortAppName = appName.Replace("Weknow.", string.Empty)
                                         .Replace("Backend.", string.Empty);
            string shortEnv = env switch
            {
                "Production" => "Prod",
                "Development" => "Dev",
                _ => env
            };

            IConnectionMultiplexer redisConnection = RedisClientFactory.CreateProviderBlocking();
            services.AddSingleton<IConnectionMultiplexer>(redisConnection);

            services.AddOptions(); // enable usage of IOptionsSnapshot<TConfig> dependency injection

            services.AddOpenTelemetryWeknow(hostEnv, shortAppName, redisConnection);

            return services;
        }

        #endregion // AddStandardWeknow

        #region AddOpenTelemetryWeknow

        /// <summary>
        /// Adds the weknow open-telemetry binding.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="hostEnv">The host env.</param>
        /// <param name="shortAppName">Short name of the application.</param>
        /// <param name="redisConnection">The redis connection.</param>
        /// <returns></returns>
        private static IServiceCollection AddOpenTelemetryWeknow(
            this IServiceCollection services,
            IHostEnvironment hostEnv,
            string shortAppName,
            IConnectionMultiplexer redisConnection)
        {
            var jaegerEndPointEnv = Environment.GetEnvironmentVariable(JAEGER_ENDPOINT); // Learn more: https://www.jaegertracing.io/docs/1.21/opentelemetry/
            var jaegerEndPoint = jaegerEndPointEnv ?? JAEGER_DEFAULT; // Learn more: https://www.jaegertracing.io/docs/1.21/opentelemetry/
            Console.WriteLine($"JAEGER endpoint: key='{JAEGER_ENDPOINT}', value='{jaegerEndPoint}', env='{jaegerEndPointEnv}'"); // will be visible in the pods logs

            services.AddOpenTelemetryTracing(builder =>
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
                    .AddRedisInstrumentation(redisConnection
                            //, m => { 
                            //    m.FlushInterval
                            //}
                            )
                    .AddOtlpExporter(options => options.Endpoint = new System.Uri(jaegerEndPoint))
                    .SetSampler(TestSampler.Create(LogLevel.Information));
                ;
                if (hostEnv.IsDevelopment())
                {
                    builder.AddConsoleExporter(options => options.Targets = ConsoleExporterOutputTargets.Console);
                }
            });
            return services;
        }

        #endregion // AddOpenTelemetryWeknow

        #region OpenTelemetryFilter

        public static bool OpenTelemetryFilter(HttpContext context) => OpenTelemetryFilter(context.Request.Path.Value);

        /// <summary>
        /// Open Telemetry Filters.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static bool OpenTelemetryFilter(string? path)
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

        #region UseStandardWeknow

        /// <summary>
        /// Uses the standard endpoints setting.
        /// </summary>
        /// <param name="endpoints">The endpoints.</param>
        /// <returns></returns>
        public static IEndpointRouteBuilder UseStandardWeknow(
                    this IEndpointRouteBuilder endpoints)
        {
            return endpoints;
        }

        #endregion UseStandardWeknow

        #region WithStandardWeknow

        /// <summary>
        /// Set Controller's with the weknow standard configuration.
        /// </summary>
        /// <param name="controllers">The controllers.</param>
        /// <returns></returns>
        public static IMvcBuilder WithStandardWeknow(
            this IMvcBuilder controllers)
        {
            return controllers.AddJsonOptions(options =>
            {
                // https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to
                JsonSerializerOptions setting = options.JsonSerializerOptions;
                setting.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                setting.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                // setting.PropertyNameCaseInsensitive = true;
                // setting.IgnoreNullValues = true;
                setting.WriteIndented = true;
                setting.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                setting.Converters.Add(JsonImmutableDictionaryConverter.Default);
            });
        }

        #endregion // WithStandardWeknow

        #region ConfigureHttpClientWeknow

        /// <summary>
        /// Configures the weknow HTTP client.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IHttpClientBuilder ConfigureHttpClientWeknow(
            this IHttpClientBuilder builder)
        {
            //AppContext.SetSwitch(
            //    "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var loggerFactory = LoggerFactory.Create(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Debug);
            });

            builder
                .ConfigureHttpClient(client =>
                    {
                        client.Timeout = TimeSpan.FromSeconds(20);
                    });
            // .EnableCallContextPropagation(); // CAUSE ERROR ON CALLS 
            //.ConfigurePrimaryHttpMessageHandler(() =>
            //    {
            //        var handler = new HttpClientHandler
            //        {
            //            // SslProtocols = SslProtocols.Tls12      

            //        };
            //        // handler.ClientCertificates.Add(LoadCertificate());
            //        return handler;
            //    });


            return builder;
        }

        #endregion // ConfigureHttpClientWeknow

        #region UseRestDefaultsWeknow

        /// <summary>
        /// Pre-configured host defaults for rest API.
        /// </summary>
        /// <typeparam name="TStartup">The type of the startup.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="args">The process arguments.</param>
        /// <returns></returns>
        public static IHostBuilder UseRestDefaultsWeknow<TStartup>(
            this IHostBuilder builder,
            params string[] args) where TStartup : class
        {
            builder.CofigureStandardWeknow(args)
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
                            //options.AddConsoleExporter();
                        });
                        //builder.AddOpenTelemetry(options =>
                        //{
                        //    options.IncludeScopes = true;
                        //    options.ParseStateValues = true;
                        //    options.IncludeFormattedMessage = true;
                        //    options.AddOtlpExporter(options => options.Endpoint = new System.Uri(jaegerEndPoint));
                        //    if (hostEnv.IsDevelopment())
                        //    {
                        //        builder.AddConsoleExporter(options => options.Targets = ConsoleExporterOutputTargets.Console);
                        //    }
                        //    options.AddConsoleExporter();
                        //});
                    });
            return builder;
        }

        #endregion // UseRestDefaultsWeknow

        #region UseGrpcDefaultsWeknow

        /// <summary>
        /// Pre-configured host defaults for gRpc services.
        /// </summary>
        /// <typeparam name="TStartup">The type of the startup.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="args">The process arguments.</param>
        /// <returns></returns>
        public static IHostBuilder UseGrpcDefaultsWeknow<TStartup>(
            this IHostBuilder builder,
            params string[] args) where TStartup : class
        {
            builder.CofigureStandardWeknow(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(
                    (WebHostBuilderContext context,
                     KestrelServerOptions options) =>
                    {
                        options.ConfigureEndpointDefaults(c => c.Protocols = HttpProtocols.Http2);
                    });
                    webBuilder.UseStartup<TStartup>();
                });
            return builder;
        }

        #endregion // UseGrpcDefaultsWeknow

        #region ConfigureWeknow

        /// <summary>
        /// Configures weknow service.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        public static IApplicationBuilder ConfigureWeknow(
                this IApplicationBuilder app,
                IWebHostEnvironment env,
                ILogger logger)
        {
            logger.LogInformation("Logging configuration setup...");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseOpenAPIWeknow();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.UseStandardWeknow();
                endpoints.MapControllers();
            });

            return app;
        }

        #endregion // ConfigureWeknow
    }
}
