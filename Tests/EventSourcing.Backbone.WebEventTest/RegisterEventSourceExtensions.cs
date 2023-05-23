using EventSourcing.Backbone;

using EventSourcing.Backbone.WebEventTest;

// Configuration: https://medium.com/@gparlakov/the-confusion-of-asp-net-configuration-with-environment-variables-c06c545ef732


namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Weknow core extensions for ASP.NET Core
    /// </summary>
    public static class RegisterEventSourceExtensions
    {
        private const string URI = "demo";

        public static IServiceCollection AddEventSource(
            this IServiceCollection services,
            Env env)
        {
            services.AddSingleton(ioc =>
            {
                ILogger logger = ioc.GetService<ILogger<Program>>() ?? throw new ArgumentNullException();
                IRawProducer producer = ProducerBuilder.Empty.GetRedisChannelService(ioc)
                                     // .AddS3Strategy(new S3Options { EnvironmentConvension = S3EnvironmentConvention.BucketPrefix })
                                     .BuildRaw();
                return producer;
            });
            services.AddSingleton(ioc =>
            {
                ILogger logger = ioc.GetService<ILogger<Program>>() ?? throw new ArgumentNullException();
                IEventFlowProducer producer = ProducerBuilder.Empty.GetRedisChannelService(ioc)
                                     // .AddS3Strategy(new S3Options { EnvironmentConvension = S3EnvironmentConvention.BucketPrefix })
                                     .Environment(env)
                                     .Uri(URI)
                                     .WithLogger(logger)
                                     .BuildEventFlowProducer();
                return producer;
            });
            services.AddSingleton(ioc =>
            {
                IConsumerReadyBuilder consumer =
                           ioc.UseRedisChannelInjection()
                                     // .AddS3Strategy(new S3Options { EnvironmentConvension = S3EnvironmentConvention.BucketPrefix })
                                     .WithOptions(o => o with
                                     {
                                         TraceAsParent = TimeSpan.FromMinutes(10),
                                         OriginFilter = MessageOrigin.Original
                                     })
                                     .Environment(env)
                                     .Uri(URI);
                return consumer;
            });
            services.AddSingleton(ioc =>
            {
                IConsumerHooksBuilder consumer =
                            ioc.UseRedisChannelInjection()
                                     // .AddS3Strategy(new S3Options { EnvironmentConvension = S3EnvironmentConvention.BucketPrefix })
                                     .WithOptions(o => o with
                                     {
                                         TraceAsParent = TimeSpan.FromMinutes(10),
                                         OriginFilter = MessageOrigin.Original
                                     });
                return consumer;
            });

            return services;
        }
    }
}
