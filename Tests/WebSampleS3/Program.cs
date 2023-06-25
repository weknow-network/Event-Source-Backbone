using Amazon.S3;

using WebSample.Extensions;

using WebSampleS3;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddEventSourceRedisConnection();

// Add services to the container.

builder.AddOpenTelemetryEventSourcing();

string URI = "shipment-tracking";
// make sure to create the bucket on AWS S3 with both prefix 'dev.' and 'prod.' and any other environment you're using (like staging,etc.) 
string s3Bucket = "shipment-tracking-sample";


builder.AddShipmentTrackingProducer(URI, s3Bucket);
builder.AddShipmentTrackingConsumer(URI, s3Bucket);

builder.Services.AddHostedService<ConsumerJob>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
