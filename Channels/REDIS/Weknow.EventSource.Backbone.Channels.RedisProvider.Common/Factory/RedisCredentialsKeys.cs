using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using Weknow.EventSource.Backbone;

using static Weknow.EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;


namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Environment keys for REDIS's credentials
    /// </summary>
    public record struct RedisCredentialsKeys
    {
        /// <summary>
        /// Endpoint Key
        /// </summary>
        public string EndpointKey { get; init; } = END_POINT_KEY;
        /// <summary>
        /// Password Key
        /// </summary>
        public string PasswordKey { get; init; } = PASSWORD_KEY;
    }
}
