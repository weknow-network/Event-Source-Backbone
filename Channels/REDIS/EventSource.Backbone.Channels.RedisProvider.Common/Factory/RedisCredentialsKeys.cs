using static EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;


namespace EventSource.Backbone
{
    /// <summary>
    /// Environment keys for REDIS's credentials
    /// </summary>
    public readonly record struct RedisCredentialsKeys
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
