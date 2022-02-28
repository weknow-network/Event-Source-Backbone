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
        public RedisCredentialsKeys()
        {
            EndpointKey = END_POINT_KEY;
            PasswordKey = PASSWORD_KEY;
        }

        /// <summary>
        /// Endpoint Key
        /// </summary>
        public string EndpointKey { get; init; }
        /// <summary>
        /// Password Key
        /// </summary>
        public string PasswordKey { get; init; }
    }
}
