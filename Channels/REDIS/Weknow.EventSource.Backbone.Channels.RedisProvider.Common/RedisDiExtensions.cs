using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

using Weknow.EventSource.Backbone;

using static Weknow.EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;

namespace Microsoft.Extensions.Configuration
{
    public static class RedisDiExtensions
    {
        public static IServiceCollection AddEventSourceRedisConnection(
            this IServiceCollection services)
        {
            services.AddSingleton<IEventSourceRedisConnectionFacroty, EventSourceRedisConnectionFacroty>();
            return services;
        }

    }
}
