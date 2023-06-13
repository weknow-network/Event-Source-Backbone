using EventSourcing.Backbone;

// Configuration: https://medium.com/@gparlakov/the-confusion-of-asp-net-configuration-with-environment-variables-c06c545ef732


namespace WebSampleS3;

/// <summary>
///  DI Extensions for ASP.NET Core
/// </summary>
public static class ShipmentTrackingProducerExtensions
{
    /// <summary>
    /// Adds the shipment tracking producer.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="uri">The URI.</param>
    /// <param name="s3Bucket">The s3 bucket.</param>
    /// <returns></returns>
    public static WebApplicationBuilder AddShipmentTrackingProducer
        (
        this WebApplicationBuilder builder,
        string uri,
        string s3Bucket)
    {
        IWebHostEnvironment environment = builder.Environment;
        string env = environment.EnvironmentName;
        IServiceCollection services = builder.Services;

        var s3Options = new S3Options { Bucket = s3Bucket };
        services.AddSingleton(ioc =>
        {
            ILogger logger = ioc.GetService<ILogger<Program>>() ?? throw new EventSourcingException("Logger is missing");
            IShipmentTrackingProducer producer = ioc.ResolveRedisProducerChannel()
                                   .ResolveS3Storage(s3Options)
                                 .Environment(env)
                                 .Uri(uri)
                                 .WithLogger(logger)
                                 .BuildShipmentTrackingProducer();
            return producer;
        });

        return builder;
    }
}
