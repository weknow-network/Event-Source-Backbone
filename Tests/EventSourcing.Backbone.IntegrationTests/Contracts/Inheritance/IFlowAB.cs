

namespace EventSourcing.Backbone.Tests.Entities;

[EventsContract(EventsContractType.Producer)]
[EventsContract(EventsContractType.Consumer)]
public interface IFlowAB : IFlowA, IFlowB
{
    ValueTask DerivedAsync(string key);
}
