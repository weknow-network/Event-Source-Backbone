using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSwag;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using StackExchange.Redis;

namespace Weknow.EventSource.Backbone.WebEventTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventSourceApiController : ControllerBase
    {
        private const string TENANT_KEY = "tenant";
        private readonly ILogger _logger;
        private readonly IEventFlowProducer _eventFlow;
        private readonly IDatabase _db;

        public EventSourceApiController(
            ILogger<EventSourceApiController> logger,
            IEventFlowProducer eventFlow,
            IConnectionMultiplexer redis)
        {
            _logger = logger;
            _eventFlow = eventFlow;

            _db = redis.GetDatabase();
        }

        [AllowAnonymous]
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<string> GetAsync()
        {
            await _eventFlow.Stage1Async(new Person(3, "Hana"), "Stage 1 data");
            var key = $"Hash:{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            await _db.HashSetAsync(key, "A", 3);
            var a = await _db.HashGetAsync(key, "A");
            return $"Hi {a}";
        }

        [AllowAnonymous]
        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task PostAsync(Person user)
        {
            await _eventFlow.Stage1Async(user, "Stage 1 data");
        }
    }
}

