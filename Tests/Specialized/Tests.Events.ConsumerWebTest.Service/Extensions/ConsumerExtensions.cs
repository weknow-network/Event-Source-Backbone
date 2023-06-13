using EventSourcing.Backbone;

// Configuration: https://medium.com/@gparlakov/the-confusion-of-asp-net-configuration-with-environment-variables-c06c545ef732

namespace Tests.Events.ConsumerWebTest;

/// <summary>
///  DI Extensions for ASP.NET Core
/// </summary>
public static class ConsumerExtensions
{
    /// <summary>
    /// Register a consumer.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="uri">The URI.</param>
    /// <returns></returns>
    public static WebApplicationBuilder AddConsumer(
        this WebApplicationBuilder builder,
        string uri)
    {
        IServiceCollection services = builder.Services;
        IWebHostEnvironment environment = builder.Environment;
        string env = environment.EnvironmentName;

        services.AddSingleton(ioc =>
        {
            return BuildConsumer(uri, env, ioc
            );
        });

        return builder;
    }

    /// <summary>
    /// Register a consumer when the URI of the service used as the registration's key.
    /// See: https://medium.com/weknow-network/keyed-dependency-injection-using-net-630bd73d3672
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="uri">The URI of the stream (which is also used as the DI key).</param>
    /// <returns></returns>
    public static WebApplicationBuilder AddKeyedConsumer(
        this WebApplicationBuilder builder,
        string uri)
    {
        IServiceCollection services = builder.Services;
        IWebHostEnvironment environment = builder.Environment;
        string env = environment.EnvironmentName;

        services.AddKeyedSingleton(ioc =>
        {
            return BuildConsumer(uri
                                , env
                                , ioc
                                );
        }, uri);

        return builder;
    }

    /// <summary>
    /// Builds the consumer.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <param name="env">The environment.</param>
    /// <param name="ioc">The DI provider.</param>
    /// <returns></returns>
    private static IConsumerReadyBuilder BuildConsumer(string uri
                                                        , Env env, IServiceProvider ioc
                                                        )
    {
        return ioc.ResolveRedisConsumerChannel()
                        .WithOptions(o => o with
                        {
                            TraceAsParent = TimeSpan.FromMinutes(10),
                            OriginFilter = MessageOrigin.Original,
                            AckBehavior = AckBehavior.OnSucceed
                        })
                        .Environment(env)
                        .Uri(uri);
    }
}
