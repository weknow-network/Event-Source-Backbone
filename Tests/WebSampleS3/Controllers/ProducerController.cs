using EventSourcing.Backbone;

using Microsoft.AspNetCore.Mvc;

namespace WebSampleS3.Controllers;

public record Ord(User user, Product Product);

[ApiController]
[Route("[controller]")]
public class ProducerController : ControllerBase
{
    private readonly ILogger<ProducerController> _logger;
    private readonly IShipmentTrackingProducer _producer;

    public ProducerController(
        ILogger<ProducerController> logger,
        IShipmentTrackingProducer producer)
    {
        _logger = logger;
        _producer = producer;
    }

    /// <summary>
    /// Post order state.
    /// </summary>
    /// <param name="payload">The payload.</param>
    /// <returns></returns>
    [HttpPost("order-placed")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    //[AllowAnonymous]
    public async Task<string> PostOrderPlacedAsync(Ord payload)
    {
        var (user, product) = payload;
        _logger.LogDebug("Sending order-placed event");
        EventKey id = await _producer.OrderPlacedAsync(user, product, DateTimeOffset.Now);
        return id;
    }

    /// <summary>
    /// Post packing state.
    /// </summary>
    /// <param name="payload">The payload.</param>
    /// <returns></returns>
    [HttpPost("packing")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<string> PostPackingAsync([FromBody] (string email, int productId) payload)
    {
        var (email, productId) = payload;
        _logger.LogDebug("Sending packing event");
        EventKey id = await _producer.PackingAsync(email, productId, DateTimeOffset.Now);
        return id;
    }

    /// <summary>
    /// Post on-delivery state.
    /// </summary>
    /// <param name="payload">The payload.</param>
    /// <returns></returns>
    [HttpPost("on-delivery")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<string> PostOnDeliveryAsync([FromBody] (string email, int productId) payload)
    {
        var (email, productId) = payload;
        _logger.LogDebug("Sending on-delivery event");
        EventKey id = await _producer.OnDeliveryAsync(email, productId, DateTimeOffset.Now);
        return id;
    }

    /// <summary>
    /// Post on-received state.
    /// </summary>
    /// <param name="payload">The payload.</param>
    /// <returns></returns>
    [HttpPost("on-received")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<string> PostOnReceivedAsync([FromBody] (string email, int productId) payload)
    {
        var (email, productId) = payload;
        _logger.LogDebug("Sending on-received event");
        EventKey id = await _producer.OnReceivedAsync(email, productId, DateTimeOffset.Now);
        return id;
    }
}