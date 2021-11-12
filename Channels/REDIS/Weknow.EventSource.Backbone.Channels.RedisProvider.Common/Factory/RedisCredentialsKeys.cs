using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using Weknow.EventSource.Backbone;

using static Weknow.EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;


namespace Weknow.EventSource.Backbone
{
    /// <summary>
    ///Redis keys for it's cedentials
    /// </summary>
    public record RedisCredentialsKeys (
                    string EndpointKey = END_POINT_KEY,
                    string PasswordKey = PASSWORD_KEY);
}
