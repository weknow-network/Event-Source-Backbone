using EventSourcing.Backbone;

namespace EventsAbstractions;


/// <summary>
/// Event's schema definition
/// Return type of each method should be  <see cref="System.Threading.Tasks.ValueTask"/>
/// </summary>
[EventsContract(EventsContractType.Producer)]
[EventsContract(EventsContractType.Consumer)]
public interface IHelloEvents
{
    ValueTask NameAsync(string name);
    ValueTask ColorAcync(ConsoleColor color);
    ValueTask StarAsync();
}