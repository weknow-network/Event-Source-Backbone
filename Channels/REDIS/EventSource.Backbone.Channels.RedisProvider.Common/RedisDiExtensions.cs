using Microsoft.Extensions.DependencyInjection;

using EventSource.Backbone;

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
