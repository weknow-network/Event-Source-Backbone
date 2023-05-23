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
        /// Initializes a new instance of the <see cref="EventSourceRedisConnectionFacroty" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public EventSourceRedisConnectionFacroty(
            ILogger logger,
            ConfigurationOptions? configuration = null) :
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
            IRedisCredentials credential,
            ILogger logger,
            Action<ConfigurationOptions>? configurationHook = null) :
                base(credential, logger, configurationHook)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSourceRedisConnectionFacroty"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="endpoint">The raw endpoint (not an environment variable).</param>
        /// <param name="password">The password (not an environment variable).</param>
        /// <param name="configurationHook">The configuration hook.</param>
        public EventSourceRedisConnectionFacroty(
            ILogger logger,
            string endpoint,
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
