using System.Text.Json;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.UnitTests.Entities
{
    [GenerateEventSource(EventSourceGenType.Producer, Name = "IProducerSequenceOperations", ContractOnly = true)]
    [GenerateEventSourceBridge(EventSourceGenType.Producer, Name = "IProducerSequenceOperations")]
    [GenerateEventSource(EventSourceGenType.Producer)]
    //[GenerateEventSourceBridge(EventSourceGenType.Producer)]
    [GenerateEventSource(EventSourceGenType.Consumer, ContractOnly = true)]
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
