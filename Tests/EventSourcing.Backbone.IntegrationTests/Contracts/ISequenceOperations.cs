

namespace EventSourcing.Backbone.Tests.Entities;

/// <summary>
/// The sequence operations.
/// </summary>
//[EventsContract(EventSourceGenType.Producer, Name = "IProducerSequenceOperations", ContractOnly = true)]
[EventsContract(EventsContractType.Producer, Name = "IProducerSequenceOperations")]
[EventsContract(EventsContractType.Producer)]
//[EventsContractBridge(EventSourceGenType.Producer)]
[EventsContract(EventsContractType.Consumer)]
//[EventsContractBridge(EventSourceGenType.Consumer)]
public interface ISequenceOperations
{
    /// <summary>
    /// Registers a user.
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
