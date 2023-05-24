using EventSourcing.Backbone;

namespace PlayGroundAbstraction;

[EventsContract(EventsContractType.Producer)]
[EventsContract(EventsContractType.Consumer)]
public interface IFoo
{
    ValueTask Run();
}