namespace EventSourcing.Backbone.UnitTests.Entities
{
    [EventsContract(EventsContractType.Producer, Name = "ISequenceOfProducer")]
    [EventsContract(EventsContractType.Consumer, Name = "ISequenceOfConsumer")]
    //[EventsContract(EventsContractType.Producer)]
    //[EventsContract(EventsContractType.Consumer)]
    //[EventsContractBridge(EventSourceGenType.Producer, Name = "ISequenceOfProducer")]
    [Obsolete("Use ISequenceOfProducer or ISequenceOfConsumer", true)]
    public interface ISequenceOperations
    {
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
