namespace Weknow.EventSource.Backbone.UnitTests.Entities
{
    [GenerateEventSource(EventSourceGenType.Producer, Name = "ISequenceOfProducer")]
    [GenerateEventSource(EventSourceGenType.Consumer, Name = "ISequenceOfConsumer")]
    [GenerateEventSource(EventSourceGenType.Producer)]
    [GenerateEventSource(EventSourceGenType.Consumer)]
    //[GenerateEventSourceBridge(EventSourceGenType.Producer, Name = "ISequenceOfProducer")]
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
