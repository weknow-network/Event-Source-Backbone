namespace EventSourcing.Backbone
{
    /// <summary>
    /// Preform acknowledge (which should prevent the 
    /// message from process again by the consumer)
    /// </summary>
    /// <seealso cref="System.IAsyncDisposable" />
    public interface IAckOperations
    {
        /// <summary>
        /// Preform acknowledge (which should prevent the 
        /// message from process again by the consumer).
        /// Must be execute from a consuming scope (i.e. method call invoked by the consumer's event processing) 
        /// </summary>
        ValueTask AckAsync();

        /// <summary>
        /// Cancel acknowledge (will happen on error in order to avoid ack on succeed)
        /// </summary>
        /// Must be execute from a consuming scope (i.e. method call invoked by the consumer's event processing) 
        /// <returns></returns>
        ValueTask CancelAsync();
    }
}
