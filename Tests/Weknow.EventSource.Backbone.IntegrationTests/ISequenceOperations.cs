using System.Text.Json;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.UnitTests.Entities
{
    [GenerateEventSourceContract(EventSourceGenType.Producer, Name = "IProducerSequenceOperations")]
    [GenerateEventSourceContract(EventSourceGenType.Producer, AutoSuffix = true)]
    [GenerateEventSourceContract(EventSourceGenType.Consumer, AutoSuffix = true)]
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
