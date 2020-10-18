namespace Weknow.EventSource.Backbone
{
    public enum AckBehavior
    {
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