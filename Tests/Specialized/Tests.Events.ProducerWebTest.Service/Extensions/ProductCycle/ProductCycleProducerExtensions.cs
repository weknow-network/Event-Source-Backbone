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
    /// <param name="builder">The builder.</param>
    /// <param name="uri">The URI.</param>
    /// <returns></returns>
    public static WebApplicationBuilder AddProductCycleProducer(
        this WebApplicationBuilder builder,
        string uri)
    {
        IServiceCollection services = builder.Services;
        IWebHostEnvironment environment = builder.Environment;
        string env = environment.EnvironmentName;

        services.AddSingleton(ioc =>
        {
            return BuildProducer(uri, env, ioc
                                );
        });

        return builder;
    }

    /// <summary>
    /// Register a producer when the URI of the service used as the registration's key.
    /// See: https://medium.com/weknow-network/keyed-dependency-injection-using-net-630bd73d3672.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="uri">The URI.</param>
    /// <returns></returns>
    public static WebApplicationBuilder AddKeyedProductCycleProducer(
        this WebApplicationBuilder builder,
        string uri)
    {
        IServiceCollection services = builder.Services;
        IWebHostEnvironment environment = builder.Environment;
        string env = environment.EnvironmentName;

        services.AddKeyedSingleton(ioc =>
        {
            return BuildProducer(uri, env, ioc
            );
        }, uri);

        return builder;
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
