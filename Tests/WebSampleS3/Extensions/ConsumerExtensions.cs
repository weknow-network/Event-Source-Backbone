using EventSourcing.Backbone;

// Configuration: https://medium.com/@gparlakov/the-confusion-of-asp-net-configuration-with-environment-variables-c06c545ef732


namespace WebSample.Extensions
{
    /// <summary>
    ///  DI Extensions for ASP.NET Core
    /// </summary>
    public static class ConsumerExtensions
    {
        /// <summary>
        /// Adds the shipment tracking producer.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="uri">The URI.</param>
        /// <param name="s3Bucket">The s3 bucket.</param>
        /// <returns></returns>
        public static IServiceCollection AddShipmentTrackingConsumer
            (
            this WebApplicationBuilder builder,
            string uri,
            string s3Bucket)
        {
            IServiceCollection services = builder.Services;
            IWebHostEnvironment environment = builder.Environment;
            string env = environment.EnvironmentName;

            var s3Options = new S3ConsumerOptions { Bucket = s3Bucket };
            services.AddSingleton(ioc =>
            {
                IConsumerReadyBuilder consumer =
                           ioc.ResolveRedisConsumerChannel()
                                .ResolveS3Storage(s3Options)
                                .WithOptions(o => o with
                                {
                                    OriginFilter = MessageOrigin.Original
                                })
                                .Environment(env)
                                .Uri(uri);
                return consumer;
            });

            return services;
        }
    }
}
