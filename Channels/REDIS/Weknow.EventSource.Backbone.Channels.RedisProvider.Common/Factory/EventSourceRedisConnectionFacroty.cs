using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using Weknow.EventSource.Backbone;

using static Weknow.EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;


namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Event Source connection (for IoC)
    /// Because IConnectionMultiplexer may be used by other component, 
    /// It's more clear to wrap the IConnectionMultiplexer for easier resove by IoC.
    /// This factory is also responsible of the connection health.
    /// It will return same connection as long as it healthy.
    /// </summary>
    public sealed class EventSourceRedisConnectionFacroty : RedisConnectionFacrotyBase, IEventSourceRedisConnectionFacroty
    {
        #region Ctor

        #region Overloads

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        /// <param name="credentialsKeys">Environment keys of the credentials</param>
        public EventSourceRedisConnectionFacroty(
            ILogger<EventSourceRedisConnectionFacroty> logger,
            Action<ConfigurationOptions>? configuration = null,
            RedisCredentialsKeys credentialsKeys = default
            ) : this((ILogger)logger, configuration, credentialsKeys)
        {
        }

        #endregion // Overloads

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        /// <param name="credentialsKeys">Environment keys of the credentials</param>
        public EventSourceRedisConnectionFacroty(
            ILogger logger,
            Action<ConfigurationOptions>? configuration = null,
            RedisCredentialsKeys credentialsKeys = default): base(logger, configuration, credentialsKeys)
        {
            //CredentialsKeys = credentialsKeys;
        }


        #endregion // Ctor

        #region Kind

        /// <summary>
        /// Gets the kind.
        /// </summary>
        protected override string Kind => "Event-Sourcing";

        #endregion // Kind

        //#region CredentialsKeys

        ///// <summary>
        ///// Gets the credentials keys.
        ///// </summary>
        //protected override RedisCredentialsKeys CredentialsKeys { get; }

        //#endregion // CredentialsKeys
    }
}
