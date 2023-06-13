using Tests.Events.ConsumerWebTest;
using Tests.Events.WebTest.Abstractions;
using Tests.Events.ConsumerWebTest.Controllers;
using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ###############  EVENT SOURCING CONFIGURATION STARTS ############################

var services = builder.Services;


IWebHostEnvironment environment = builder.Environment;
string env = environment.EnvironmentName;

services.AddOpenTelemetry()
        .WithEventSourcingTracing(environment)
        .WithEventSourcingMetrics(environment);

services.AddEventSourceRedisConnection();
builder.AddKeyedConsumer(ProductCycleConstants.URI);

services.AddHostedService<ConsumerJob>();


// ###############  EVENT SOURCING CONFIGURATION ENDS ############################

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
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
<p>docker run --name jaeger-otel  --rm -it -e COLLECTOR_OTLP_ENABLED=true -p 16686:16686 -p 4317:4317 -p 4318:4318  jaegertracing/all-in-one:latest</p>
",
        });
    }); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var logger = app.Services.GetService<ILogger<Program>>();
List<string> switches = new();
switches.Add("Consumer");
logger.LogInformation("Service Configuration Event Sourcing `{event-bundle}` on URI: `{URI}`, Features: [{features}]", "ProductCycle", ProductCycleConstants.URI, string.Join(", ", switches));
    
app.Run();
