using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

using Weknow.EventSource.Backbone;

using static Weknow.EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;

namespace Microsoft.Extensions.Configuration
{
    public static class RedisDiExtensions
    {
        public static IServiceCollection AddEventSourceRedisConnection(
            this IServiceCollection services,
                    Action<ConfigurationOptions>? configuration = null,
                    string endpointKey = END_POINT_KEY,
                    string passwordKey = PASSWORD_KEY)
        {
            var redis = RedisClientFactory.CreateProviderAsync(configuration, endpointKey, passwordKey);
            var conn = new EventSourceRedisConnection(redis);
            services.AddSingleton(conn);
            return services;
        }

    }
}
