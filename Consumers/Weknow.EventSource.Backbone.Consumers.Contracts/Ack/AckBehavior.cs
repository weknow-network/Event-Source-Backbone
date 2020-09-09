namespace Weknow.EventSource.Backbone
{
    public enum AckBehavior
    {
        /// <summary>
        /// The message won't send again after first delivery.
        /// </summary>
        FireAndForget,
        /// <summary>
        /// Automatic acknowledge when execute without exception.
        /// </summary>
        OnSucceed,
        /// <summary>
        /// Ignored expected to be handle elsewhere.
        /// </summary>
        Manual,
    }
}