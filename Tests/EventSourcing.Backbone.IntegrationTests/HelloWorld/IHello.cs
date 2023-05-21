namespace EventSourcing.Backbone.UnitTests.Entities
{
    /// <summary>
    /// The sequence operations.
    /// </summary>
    [GenerateEventSource(EventSourceGenType.Producer)]
    [GenerateEventSource(EventSourceGenType.Consumer)]
    public interface IHello
    {
        ValueTask HelloAsync(string message);
        ValueTask WorldAsync(int value);
    }
}
