using EventSourcing.Backbone;

using PlayGroundAbstraction.Common.Local;

namespace PlayGroundAbstraction.Common;



[EventsContract(EventsContractType.Producer)]
[EventsContract(EventsContractType.Consumer)]
public interface IFoo
{
    ValueTask Run(User user);
}