// Configuration: https://medium.com/@gparlakov/the-confusion-of-asp-net-configuration-with-environment-variables-c06c545ef732

namespace EventSourcing.Backbone.WebEventTest;

/// <summary>
///  DI Extensions for ASP.NET Core
/// </summary>
public static class ConsumerExtensions
{
    /// <summary>
    /// Adds the shipment tracking producer.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="uri">The URI.</param>
    /// <param name="s3Bucket">The s3 bucket.</param>
    /// <param name="env">The environment.</param>
    /// <returns></returns>
    public static IServiceCollection AddConsumer
        (
        this IServiceCollection services,
        string uri,
        string s3Bucket,
        Env env)
    {
        var s3Options = new S3Options { Bucket = s3Bucket };
        services.AddKeyedSingleton(ioc =>
        {
            IConsumerReadyBuilder consumer =
                        ioc.ResolveRedisConsumerChannel()
                            .ResolveS3Storage(s3Options)
                            .WithOptions(o => o with
                            {
                                OriginFilter = MessageOrigin.Original,
                                AckBehavior = AckBehavior.OnSucceed,
                                //TelemetryLevel = LogLevel.Debug 
                            })
                            .Environment(env)
                            .Uri(uri);
            return consumer;
        }, EventSourcingConstants.URI);

        return services;
    }
}
