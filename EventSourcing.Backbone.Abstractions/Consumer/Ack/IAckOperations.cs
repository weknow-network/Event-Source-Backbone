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
        /// <param name="cause">The cause of the acknowledge.</param>
        /// <returns></returns>
        ValueTask AckAsync(AckBehavior cause = AckBehavior.Manual);

        /// <summary>
        /// Cancel acknowledge (will happen on error in order to avoid ack on succeed)
        /// </summary>
        /// Must be execute from a consuming scope (i.e. method call invoked by the consumer's event processing) 
        /// <param name="cause">The cause of the cancellation.</param>
        /// <returns></returns>
        ValueTask CancelAsync(AckBehavior cause = AckBehavior.Manual);
    }
}
