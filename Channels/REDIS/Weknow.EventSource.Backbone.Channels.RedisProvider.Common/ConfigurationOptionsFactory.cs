using System;
using StackExchange.Redis;

namespace Weknow.EventSource.Backbone.Private
{
    /// <summary>
    /// Configuration options factory
    /// </summary>
    public static class ConfigurationOptionsFactory
    {
        /// <summary>
        /// Create configuration from environment.
        /// </summary>
        /// <param name="endpointKey">The endpoint key.</param>
        /// <param name="passwordKey">The password key.</param>
        /// <returns></returns>
        public static ConfigurationOptions FromEnv(
                            string endpointKey,
                            string passwordKey)
        {
            string endpoint = Environment.GetEnvironmentVariable(endpointKey) ?? "localhost";
            string? password = Environment.GetEnvironmentVariable(passwordKey);

            var redisConfiguration = ConfigurationOptions.Parse(endpoint);
            redisConfiguration.Password = password;

            return redisConfiguration;
        }
    }
}
