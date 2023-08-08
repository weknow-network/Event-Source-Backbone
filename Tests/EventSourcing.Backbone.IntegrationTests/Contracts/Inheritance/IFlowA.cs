namespace EventSourcing.Backbone.Tests.Entities;

[EventsContract(EventsContractType.Producer)]
[EventsContract(EventsContractType.Consumer)]
public interface IFlowA
{
    ValueTask AAsync(int id);
}
