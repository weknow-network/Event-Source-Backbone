namespace EventSource.Backbone.UnitTests.Entities
{
    /// <summary>
    /// The sequence operations.
    /// </summary>
    //[GenerateEventSource(EventSourceGenType.Producer, Name = "IProducerSequenceOperations", ContractOnly = true)]
    [GenerateEventSource(EventSourceGenType.Producer, Name = "IProducerSequenceOperations")]
    [GenerateEventSource(EventSourceGenType.Producer)]
    //[GenerateEventSourceBridge(EventSourceGenType.Producer)]
    [GenerateEventSource(EventSourceGenType.Consumer)]
    //[GenerateEventSourceBridge(EventSourceGenType.Consumer)]
    public interface ISequenceOperations
    {
        /// <summary>
        /// Registers a user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        ValueTask RegisterAsync(User user);
        ValueTask UpdateAsync(User user);
        ValueTask LoginAsync(string email, string password);
        ValueTask LogoffAsync(int id);
        ValueTask ApproveAsync(int id);
        ValueTask SuspendAsync(int id);
        ValueTask ActivateAsync(int id);
        ValueTask EarseAsync(int id);
    }
}
