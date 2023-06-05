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
    /// <param name="services">The services.</param>
    /// <param name="uri">The URI.</param>
    /// <param name="s3Bucket">The s3 bucket.</param>
    /// <param name="env">The environment.</param>
    /// <returns></returns>
    public static IServiceCollection AddShipmentTrackingProducer
        (
        this IServiceCollection services,
        string uri,
        string s3Bucket,
        Env env)
    {
        var s3Options = new S3Options { Bucket = s3Bucket };
        services.AddSingleton(ioc =>
        {
            ILogger logger = ioc.GetService<ILogger<Program>>() ?? throw new EventSourcingException("Logger is missing");
            IShipmentTrackingProducer producer = ioc.ResolveRedisProducerChannel()
                                   .ResolveS3Strategy(s3Options)
                                 .Environment(env)
                                 .Uri(uri)
                                 .WithLogger(logger)
                                 .BuildShipmentTrackingProducer();
            return producer;
        });

        return services;
    }
}
