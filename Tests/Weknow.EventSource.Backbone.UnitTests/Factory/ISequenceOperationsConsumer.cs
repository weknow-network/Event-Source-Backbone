using System;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.UnitTests.Entities
{
    public interface ISequenceOperationsConsumer
    {
        ValueTask RegisterAsync(User user, Func<ValueTask> ack);
        ValueTask UpdateAsync(User user);
        ValueTask LoginAsync(string email, string password, Func<ValueTask> ack);
        ValueTask LogoffAsync(int id);
        ValueTask ApproveAsync(int id);
        ValueTask SuspendAsync(int id);
        ValueTask ActivateAsync(int id);
        ValueTask EarseAsync(int id);
    }
}
