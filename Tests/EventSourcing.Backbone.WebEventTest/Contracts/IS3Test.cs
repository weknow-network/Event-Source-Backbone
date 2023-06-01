using System.ComponentModel;
using System.Text.Json;

namespace EventSourcing.Backbone.WebEventTest
{
    [EventsContract(EventsContractType.Consumer)]
    [EventsContract(EventsContractType.Producer)]
    [Obsolete("Used for code generation, use the producer / consumer version of it", true)]
    public interface IS3Test
    {
        ValueTask NameAsync(string payload);
    }
}
