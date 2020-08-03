namespace Weknow.EventSource.Backbone
{
    public enum TimeoutBehavior
    {       
        /// <summary>
        /// Will acknowledge the messages on timeout.
        /// </summary>
        Ack,
        /// <summary>
        /// Will replay the messages on timeout.
        /// </summary>
        Replay
    }
}