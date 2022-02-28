namespace Weknow.EventSource.Backbone
{
    public enum PartialConsumerBehavior
    {
        /// <summary>
        /// Don't allow by-pass event (consumer must handle all events)
        /// Will throw if event had bypassed.
        /// </summary>
        Strict,
        /// <summary>
        /// Allow by-pass event (consumer must handle all events)
        /// Will acknowledge if event had bypassed.
        /// </summary>
        allow,
    }
}