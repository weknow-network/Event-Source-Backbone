using System.Text.Json;

using EventSourcing.Backbone;
using WebSampleS3;

using Microsoft.AspNetCore.Mvc;

namespace WebSampleS3.Controllers;

[ApiController]
[Route("[controller]")]
public class ConsumerController : ControllerBase
{
    private readonly ILogger<ConsumerController> _logger;
    private readonly IConsumerReceiver _receiver;

    public ConsumerController(
        ILogger<ConsumerController> logger,
        IConsumerReadyBuilder consumerBuilder)
    {
        _logger = logger;
        _receiver = consumerBuilder.BuildReceiver();
    }

    /// <summary>
    /// Gets an event by event key.
    /// </summary>
    /// <param name="eventKey">The event key.</param>
    /// <returns></returns>
    [HttpGet("{eventKey}")]
    public async Task<JsonElement> GetAsync(string eventKey)
    {
        _logger.LogDebug("fetching event [{key}]", eventKey);
        var json = await _receiver.GetJsonByIdAsync(eventKey);
        return json;
    }
}