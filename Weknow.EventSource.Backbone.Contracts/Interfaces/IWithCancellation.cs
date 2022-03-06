namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Event Source cancellation builder.
    /// </summary>
    public interface IWithCancellation<T>
    {
        /// <summary>
        /// Withes the cancellation token.
        /// </summary>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        T WithCancellation(CancellationToken cancellation);
    }
}
