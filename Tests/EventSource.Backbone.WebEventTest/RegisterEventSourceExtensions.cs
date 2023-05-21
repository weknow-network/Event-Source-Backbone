using EventSource.Backbone;

using EventSource.Backbone.WebEventTest;

// Configuration: https://medium.com/@gparlakov/the-confusion-of-asp-net-configuration-with-environment-variables-c06c545ef732


namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Weknow core extensions for ASP.NET Core
    /// </summary>
    public static class RegisterEventSourceExtensions
    {
        private const string PARTITION = "demo";

        public static IServiceCollection AddEventSource(
            this IServiceCollection services,
            Env env)
        {
            services.AddSingleton(ioc =>
            {
                ILogger logger = ioc.GetService<ILogger<Program>>() ?? throw new ArgumentNullException();
                IRawProducer producer = ProducerBuilder.Empty.UseRedisChannelInjection(ioc)
                                     // .AddS3Strategy(new S3Options { EnvironmentConvension = S3EnvironmentConvention.BucketPrefix })
                                     .BuildRaw();
                return producer;
            });
            services.AddSingleton(ioc =>
            {
                ILogger logger = ioc.GetService<ILogger<Program>>() ?? throw new ArgumentNullException();
                IEventFlowProducer producer = ProducerBuilder.Empty.UseRedisChannelInjection(ioc)
                                     // .AddS3Strategy(new S3Options { EnvironmentConvension = S3EnvironmentConvention.BucketPrefix })
                                     .Environment(env)
                                     .Uri(PARTITION)
                                     .WithLogger(logger)
                                     .BuildEventFlowProducer();
                return producer;
            });
            services.AddSingleton(ioc =>
            {
                IConsumerReadyBuilder consumer =
                            ConsumerBuilder.Empty.UseRedisChannelInjection(ioc)
                                     // .AddS3Strategy(new S3Options { EnvironmentConvension = S3EnvironmentConvention.BucketPrefix })
                                     .WithOptions(o => o with
                                     {
                                         TraceAsParent = TimeSpan.FromMinutes(10),
                                         OriginFilter = MessageOrigin.Original
                                     })
                                     .Environment(env)
                                     .Uri(PARTITION);
                return consumer;
            });
            services.AddSingleton(ioc =>
            {
                IConsumerHooksBuilder consumer =
                            ConsumerBuilder.Empty.UseRedisChannelInjection(ioc)
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
