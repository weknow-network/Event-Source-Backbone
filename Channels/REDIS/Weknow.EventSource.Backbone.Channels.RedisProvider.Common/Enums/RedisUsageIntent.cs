namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    /// <summary>
    /// REDIS Usage Intent
    /// </summary>
    public enum RedisUsageIntent
    {
        /// <summary>
        /// Read
        /// </summary>
        Read,
        /// <summary>
        /// Write
        /// </summary>
        Write,
        /// <summary>
        /// Enables a range of commands that are considered risky.
        /// </summary>
        Admin
    }
}
