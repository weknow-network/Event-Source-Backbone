namespace EventSourcing.Backbone
{
    /// <summary>
    /// Preform acknowledge (which should prevent the 
    /// message from process again by the consumer)
    /// </summary>
    /// <seealso cref="System.IAsyncDisposable" />
    public interface IAck : IAckOperations, IAsyncDisposable
    {
    }
}
