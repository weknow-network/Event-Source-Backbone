using static EventSourcing.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;


namespace EventSourcing.Backbone
{
    /// <summary>
    /// Environment keys for REDIS's credentials
    /// </summary>
    public record RedisCredentialsKeys
    {
        /// <summary>
        /// Environment key of the end-point, if missing it use a default ('REDIS_EVENT_SOURCE_ENDPOINT').
        /// If the environment variable doesn't exists, It assumed that the value represent an actual end-point and use it.
        /// </summary>
        public string Endpoint { get; init; } = END_POINT_KEY;
        /// <summary>
        /// Environment key of the password, if missing it use a default ('REDIS_EVENT_SOURCE_PASS').
        /// If the environment variable doesn't exists, It assumed that the value represent an actual password and use it.
        /// </summary>
        public string Password { get; init; } = PASSWORD_KEY;
    }
}
