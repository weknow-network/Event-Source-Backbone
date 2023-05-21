using Microsoft.Extensions.Logging;

using StackExchange.Redis;


namespace EventSourcing.Backbone
{
    /// <summary>
    /// Event Source connection (for IoC)
    /// Because IConnectionMultiplexer may be used by other component, 
    /// It's more clear to wrap the IConnectionMultiplexer for easier resove by IoC.
    /// This factory is also responsible of the connection health.
    /// It will return same connection as long as it healthy.
    /// </summary>
    public sealed class EventSourceRedisConnectionFacroty : RedisConnectionFacrotyBase
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSourceRedisConnectionFacroty"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        public EventSourceRedisConnectionFacroty(
            ILogger logger,
            ConfigurationOptions? configuration) :
                base(logger, configuration)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSourceRedisConnectionFacroty"/> class.
        /// </summary>
        /// <param name="credential">The credential.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="configurationHook">The configuration hook.</param>
        public EventSourceRedisConnectionFacroty(
            RedisCredentialsKeys credential,
            ILogger logger,
            Action<ConfigurationOptions>? configurationHook = null) :
                base(credential, logger, configurationHook)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSourceRedisConnectionFacroty"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="endpoint">Environment key of the end-point, if missing it use a default ('REDIS_EVENT_SOURCE_ENDPOINT').
        /// If the environment variable doesn't exists, It assumed that the value represent an actual end-point and use it.</param>
        /// <param name="password">Environment key of the password, if missing it use a default ('REDIS_EVENT_SOURCE_PASS').
        /// If the environment variable doesn't exists, It assumed that the value represent an actual password and use it.</param>
        /// <param name="configurationHook">The configuration hook.</param>
        public EventSourceRedisConnectionFacroty(
            ILogger logger,
            string? endpoint = null,
            string? password = null,
            Action<ConfigurationOptions>? configurationHook = null) :
                base(logger, endpoint, password, configurationHook)
        {
        }

        #endregion // Ctor

        #region Kind

        /// <summary>
        /// Gets the kind.
        /// </summary>
        protected override string Kind { get; } = "Event-Sourcing";

        #endregion // Kind
    }
}
