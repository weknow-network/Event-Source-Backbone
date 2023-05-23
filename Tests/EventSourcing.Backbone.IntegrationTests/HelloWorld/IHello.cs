namespace EventSourcing.Backbone.UnitTests.Entities
{
    /// <summary>
    /// The sequence operations.
    /// </summary>
    [EventsContract(EventsContractType.Producer)]
    [EventsContract(EventsContractType.Consumer)]
    public interface IHello
    {
        ValueTask HelloAsync(string message);
        ValueTask WorldAsync(int value);
    }
}
