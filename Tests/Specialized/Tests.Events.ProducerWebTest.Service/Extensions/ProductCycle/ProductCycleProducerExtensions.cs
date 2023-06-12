using EventSourcing.Backbone;
using Tests.Events.WebTest.Abstractions;

// Configuration: https://medium.com/@gparlakov/the-confusion-of-asp-net-configuration-with-environment-variables-c06c545ef732


namespace Tests.Events.ProducerWebTest;

/// <summary>
///  DI Extensions for ASP.NET Core
/// </summary>
public static class ProductCycleProducerExtensions
{
    /// <summary>
    /// Register a producer.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="uri">The URI.</param>
    /// <param name="env">The environment.</param>
    /// <returns></returns>
    public static IServiceCollection AddProductCycleProducer
        (
        this IServiceCollection services,
        string uri,
        Env env)
    {
        services.AddSingleton(ioc =>
        {
            return BuildProducer(uri, env, ioc
                                );
        });

        return services;
    }

    /// <summary>
    /// Register a producer when the URI of the service used as the registration's key.
    /// See: https://medium.com/weknow-network/keyed-dependency-injection-using-net-630bd73d3672.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="uri">The URI.</param>
    /// <param name="env">The environment.</param>
    /// <returns></returns>
    public static IServiceCollection AddKeyedProductCycleProducer
        (
        this IServiceCollection services,
        string uri,
        Env env)
    {
        services.AddKeyedSingleton(ioc =>
        {
            return BuildProducer(uri, env, ioc
            );
        }, uri);

        return services;
    }

    private static IProductCycleProducer BuildProducer(string uri, Env env, IServiceProvider ioc
    )
    {
        ILogger logger = ioc.GetService<ILogger<Program>>() ?? throw new EventSourcingException("Logger is missing");
        IProductCycleProducer producer = ioc.ResolveRedisProducerChannel()
                                .Environment(env)
                                .Uri(uri)
                                .WithLogger(logger)
                                .BuildProductCycleProducer();
        return producer;
    }
}
