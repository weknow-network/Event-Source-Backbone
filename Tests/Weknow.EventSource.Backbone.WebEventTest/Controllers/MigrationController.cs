using Microsoft.AspNetCore.Mvc;

namespace Weknow.EventSource.Backbone.WebEventTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MigrationController : ControllerBase, IEventsMigration
    {
        private readonly ILogger _logger;
        private readonly IRawProducer _forwarder;

        public MigrationController(
            ILogger<EventSourceApiController> logger,
            IRawProducer forwarder)
        {
            _logger = logger;
            _forwarder = forwarder;
        }

        /// <summary>
        /// Post Analysts
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPost]
        //[AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task ForwardEventAsync(Announcement announcement)
        {
            _logger.LogInformation("Processing fw message {announcement}", announcement);
            await _forwarder.Produce(announcement);
        }
    }
}

