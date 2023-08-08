namespace EventSourcing.Backbone.Tests.Entities;

[EventsContract(EventsContractType.Producer)]
[EventsContract(EventsContractType.Consumer)]
public interface IFlowB
{
    ValueTask BAsync(DateTimeOffset date);
}
