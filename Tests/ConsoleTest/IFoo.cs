using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EventSourcing.Backbone;

namespace ConsoleTest;

[EventsContract(EventsContractType.Producer)]
[EventsContract(EventsContractType.Consumer)]
public interface IFoo
{
    ValueTask Event1Async();
    ValueTask Event2Async(int i);
    ValueTask Event3Async(string message);
}
