using System.Text.Json;
using System.Text.Json.Serialization;

using EventSourcing.Backbone;

using Microsoft.AspNetCore.Server.Kestrel.Core;

using StackExchange.Redis;

// Configuration: https://medium.com/@gparlakov/the-confusion-of-asp-net-configuration-with-environment-variables-c06c545ef732


namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    ///  core extensions for ASP.NET Core
    /// </summary>
    public static class AspCoreExtensions
    {
        #region AddRedis

        /// <summary>
        /// Adds the  standard configuration.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IConnectionMultiplexer AddRedis(
            this IServiceCollection services)
        {
            IConnectionMultiplexer redisConnection = RedisClientFactory.CreateProviderAsync().Result;
            services.AddSingleton<IConnectionMultiplexer>(redisConnection);

            return redisConnection;
        }

        #endregion // AddRedis

        #region WithJsonOptions

        /// <summary>
        /// Set Controller's with the standard json configuration.
        /// </summary>
        /// <param name="controllers">The controllers.</param>
        /// <returns></returns>
        public static IMvcBuilder WithJsonOptions(
            this IMvcBuilder controllers)
        {
            return controllers.AddJsonOptions(options =>
            {
                // https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to
                JsonSerializerOptions setting = options.JsonSerializerOptions;
                setting.WithDefault();

            });
        }

        #endregion // WithJsonOptions

        #region WithDefault

        /// <summary>
        /// Withes the default.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static JsonSerializerOptions WithDefault(this JsonSerializerOptions options)
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            options.WriteIndented = true;
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            options.Converters.Add(JsonMemoryBytesConverterFactory.Default);
            return options;
        }

        #endregion // WithDefault
    }
}
