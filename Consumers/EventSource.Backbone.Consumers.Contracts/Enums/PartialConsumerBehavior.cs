namespace EventSource.Backbone.Enums
{
    /// <summary>
    /// Gets or sets the partial behavior 
    ///  - does consumer had to handle all events?.
    ///  - can it ignore event and proceed (marking the event as consumed on its Consumer Group) 
    ///    or should it wait to other consumers to consume the event.
    /// </summary>
    public enum PartialConsumerBehavior
    {
        /// <summary>
        /// Execute sequentially, Don't allow by-pass event.
        /// Events must be handled by the consumer.
        /// Unhandled event causing an exception to been thrown.
        /// </summary>
        ThrowIfNotHandled,
        /// <summary>
        /// Execute sequentially, Don't allow by-pass event.
        /// Events must be handled either by the consumer or 
        /// by other consumers in the consumer group.
        /// </summary>
        Sequential,
        /// <summary>
        /// Allow by-pass event (unhandled event automatically been acknowledged)
        /// </summary>
        Loose,
    }
}