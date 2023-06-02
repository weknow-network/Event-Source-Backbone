using System.Text.Json;

using Amazon.S3;

using EventSourcing.Backbone.WebEventTest;
using EventSourcing.Backbone.WebEventTest.Jobs;

using Microsoft.OpenApi.Models;

using Refit;

using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();

IWebHostEnvironment environment = builder.Environment;
string env = environment.EnvironmentName;
string appName = environment.ApplicationName;
string shortAppName = appName.Replace("EventSourcing.Backbone.", string.Empty);


var services = builder.Services;
// Add services to the container.

services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(
    opt =>
    {
        opt.SupportNonNullableReferenceTypes();
        opt.IgnoreObsoleteProperties();
        opt.IgnoreObsoleteActions();
        opt.DescribeAllParametersInCamelCase();

        opt.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Environment setup",
            Description = @"<p><b>Use the following docker in order to setup the environment</b></p>
<p>docker run -p 6379:6379 -it --rm --name redis-Json redislabs/rejson:latest</p>
<p>docker run --rm -it --name jaeger -p 13133:13133 -p 16686:16686 -p 4317:55680 jaegertracing/opentelemetry-all-in-one</p>
",
        });
    });
services.AddHostedService<MicroDemoJob>();
services.AddHostedService<MigrationJob>();
string fwPort = Environment.GetEnvironmentVariable("FW") ?? "MISSING-PORT";

var jsonOptions = new JsonSerializerOptions().WithDefault();

var refitSetting = new RefitSettings
{
    ContentSerializer = new SystemTextJsonContentSerializer(jsonOptions)
};
services.AddRefitClient<IEventsMigration>(refitSetting) // https://github.com/reactiveui/refit
        .ConfigureHttpClient(c =>
        {
            c.BaseAddress = new Uri($"http://localhost:{fwPort}/api/Migration");
            c.DefaultRequestHeaders.Add("wk-pattern", "migration");
        });
services.AddHttpClient("migration", c =>
 {
     c.BaseAddress = new Uri($"http://localhost:{fwPort}/api/Migration");
     c.DefaultRequestHeaders.Add("wk-pattern", "migration");
 });

IConnectionMultiplexer redisConnection = services.AddRedis(environment, shortAppName);
services.AddOpenTelemetryWeknow(environment, shortAppName, redisConnection);


services.AddOptions(); // enable usage of IOptionsSnapshot<TConfig> dependency injection

services.AddEventSourceRedisConnection();

services.AddEventSource(env);


void RedisConfigEnrichment(ConfigurationOptions configuration)
{
    configuration.ReconnectRetryPolicy = new RedisReconnectRetryPolicy();
    configuration.ClientName = "Web Test";
}
services.AddSingleton<Action<ConfigurationOptions>>(RedisConfigEnrichment);

services.AddControllers()
                .WithJsonOptions();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
