using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.UnitTests.Entities
{
    [GenerateEventSourceContract(EventSourceGenType.Producer, Name = "ISequenceOf", AutoSuffix = true)]
    public interface ISequenceOperationsConsumer
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
