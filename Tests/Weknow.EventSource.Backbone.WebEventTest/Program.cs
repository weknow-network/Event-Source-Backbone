using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.WebEventTest.Jobs;
using Weknow.EventSource.Backbone;

using Weknow.EventSource.Backbone.WebEventTest;
using StackExchange.Redis;
using Microsoft.OpenApi.Models;

const string ENV = $"test";

var builder = WebApplication.CreateBuilder(args);

IWebHostEnvironment environment = builder.Environment;
string env = environment.EnvironmentName;
string appName = environment.ApplicationName;
string shortAppName = appName.Replace("Weknow.", string.Empty)
                             .Replace("Backend.", string.Empty);


var services = builder.Services;
// Add services to the container.

services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(
    opt => {
        //opt.UseInlineDefinitionsForEnums();
        //opt.UseOneOfForPolymorphism();
        //opt.UseAllOfToExtendReferenceSchemas();
        //opt.UseAllOfForInheritance();
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
            
            //TermsOfService = new Uri("https://example.com/terms"),
            //Contact = new OpenApiContact
            //{
            //    Name = "Example Contact",
            //    Url = new Uri("https://example.com/contact")
            //},
            //License = new OpenApiLicense
            //{
            //    Name = "Example License",
            //    Url = new Uri("https://example.com/license")
            //}
        });

        //opt.SwaggerDoc("Event Source", new Microsoft.OpenApi.Models.OpenApiInfo { Description = "Bla" });
        //opt.SelectSubTypesUsing();
        //opt.SelectDiscriminatorValueUsing();
        //opt.SelectDiscriminatorNameUsing();
    });
services.AddHostedService<MicroDemoJob>();

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

//app.UseAuthorization();

app.MapControllers();

app.Run();


/// <summary>
/// Redis reconnect retry policy
/// </summary>
/// <seealso cref="StackExchange.Redis.IReconnectRetryPolicy" />
public class RedisReconnectRetryPolicy: IReconnectRetryPolicy
{
    /// <summary>
    /// Shoulds the retry.
    /// </summary>
    /// <param name="currentRetryCount">The current retry count.</param>
    /// <param name="timeElapsedMillisecondsSinceLastRetry">The time elapsed milliseconds since last retry.</param>
    /// <returns></returns>
    bool IReconnectRetryPolicy.ShouldRetry(
        long currentRetryCount,
        int timeElapsedMillisecondsSinceLastRetry)
    {
        return timeElapsedMillisecondsSinceLastRetry > 1000;
    }
}