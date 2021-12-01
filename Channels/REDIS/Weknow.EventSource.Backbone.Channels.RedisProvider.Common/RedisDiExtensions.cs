using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

using Weknow.EventSource.Backbone;

using static Weknow.EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// The redis DI extensions.
    /// </summary>
    public static class RedisDiExtensions
    {
        /// <summary>
        /// Adds the event source redis connection to the DI.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>An IServiceCollection.</returns>
        public static IServiceCollection AddEventSourceRedisConnection(
            this IServiceCollection services)
        {
            services.AddSingleton<IEventSourceRedisConnectionFacroty, EventSourceRedisConnectionFacroty>();
            return services;
        }

    }
}
