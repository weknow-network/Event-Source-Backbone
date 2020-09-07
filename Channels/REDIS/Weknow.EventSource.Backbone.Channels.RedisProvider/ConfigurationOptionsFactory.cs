using StackExchange.Redis;

using System;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    public static class ConfigurationOptionsFactory
    {
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
