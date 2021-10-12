using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.UnitTests.Entities
{
    public interface ISequenceOperationsProducer
    {
        /// <summary>
        /// Registers the asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
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
