#pragma warning disable S1133 // Deprecated code should be removed

using EventSourcing.Backbone;

namespace WebSampleS3;


/// <summary>
/// Event's schema definition
/// Return type of each method should be  <see cref="System.Threading.Tasks.ValueTask"/>
/// </summary>
[EventsContract(EventsContractType.Producer)]
[EventsContract(EventsContractType.Consumer)]
[Obsolete("Either use the Producer or Consumer version of this interface", true)]
public interface IShipmentTracking
{
    // the following method will be ValueTask<EventKeys> OrderPlacedAsync(...) at the producer interface and ValueTask OrderPlacedAsync(...) at the consumer
    void OrderPlaced(User user, Product product, DateTimeOffset time);
    ValueTask PackingAsync(string email, int productId, DateTimeOffset time);
    ValueTask OnDeliveryAsync(string email, int productId, DateTimeOffset time);
    ValueTask OnReceivedAsync(string email, int productId, DateTimeOffset time);
}