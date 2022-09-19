namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// The message origin
    /// </summary>
    public enum MessageOrigin
    {
        /// <summary>
        /// Original message (not forwarded from other source)
        /// </summary>
        Original,
        /// <summary>
        /// Copied from other source (see linked meta)
        /// </summary>
        Copy
    }
}
