namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// The message origin
    /// </summary>
    [Flags]
    public enum MessageOrigin
    {
        Unknown = 0,
        /// <summary>
        /// Original message (not forwarded from other source)
        /// </summary>
        Original = 1,
        /// <summary>
        /// Copied from other source (see linked meta)
        /// </summary>
        Copy = Original * 2
    }
}
