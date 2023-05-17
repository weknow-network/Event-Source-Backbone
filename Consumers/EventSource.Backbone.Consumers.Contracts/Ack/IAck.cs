namespace EventSource.Backbone
{
    /// <summary>
    /// Preform acknowledge (which should prevent the 
    /// message from process again by the consumer)
    /// </summary>
    /// <seealso cref="System.IAsyncDisposable" />
    public interface IAck : IAsyncDisposable
    {
        /// <summary>
        /// Preform acknowledge (which should prevent the 
        /// message from process again by the consumer)
        /// </summary>
        ValueTask AckAsync();

        /// <summary>
        /// Cancel acknowledge (will happen on error in order to avoid ack on succeed)
        /// </summary>
        /// <returns></returns>
        ValueTask CancelAsync();
    }
}
