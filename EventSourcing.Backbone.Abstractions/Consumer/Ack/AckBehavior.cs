namespace EventSourcing.Backbone
{
    public enum AckBehavior
    {
        /// <summary>
        /// Automatic acknowledge when execute without exception.
        /// </summary>
        OnSucceed,
        /// <summary>
        /// Automatic acknowledge when execute complete (whether it succeed or having exception, like finaly) .
        /// </summary>
        OnFinally,
        /// <summary>
        /// Ignored expected to be handle elsewhere.
        /// </summary>
        Manual,
        /// <summary>
        /// Manual acknowledge with fallback state indication.
        /// </summary>
        OnFallback,
    }
}