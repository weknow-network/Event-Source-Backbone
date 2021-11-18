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
using System.Text.Json;
using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone.WebEventTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventSourceApiController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IEventFlowProducer _eventFlowProducer;
        private readonly IConsumerReadyBuilder _consumerBuilder;
        private readonly IConsumerHooksBuilder _baseBuilder;
        private readonly IDatabase _db;

        public EventSourceApiController(
            ILogger<EventSourceApiController> logger,
            IEventFlowProducer eventFlowProducer,
            IConsumerReadyBuilder consumerBuilder,
            Building.IConsumerHooksBuilder baseBuilder,
            IConnectionMultiplexer redis)
        {
            _logger = logger;
            _eventFlowProducer = eventFlowProducer;
            _consumerBuilder = consumerBuilder;
            _baseBuilder = baseBuilder;
            _db = redis.GetDatabase();
        }

        #region GetAsync

        /// <summary>
        /// Post Analysts
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpGet("{eventKey}")]
        //[AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async ValueTask<JsonElement> GetAsync(string eventKey)
        {
            var receiver = _consumerBuilder.BuildReceiver();
            var json = await receiver.GetJsonByIdAsync(eventKey);
            return json;
        }

        /// <summary>
        /// Post Analysts
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpGet("basic/{eventKey}/{env}")]
        //[AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async ValueTask<JsonElement> GetAsync(string eventKey, string env)
        {
            var receiver = _consumerBuilder.Environment(env).BuildReceiver();
            var json = await receiver.GetJsonByIdAsync(eventKey);
            return json;
        }

        /// <summary>
        /// Post Analysts
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpGet("more/{partition}/{shard}/{eventKey}/{env?}")]
        //[AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async ValueTask<JsonElement> GetMoreAsync(string partition, string shard, string eventKey, string? env = null)
        {
            var receiver = _baseBuilder.Partition(partition).Shard(shard).Environment(env).BuildReceiver();
            var json = await receiver.GetJsonByIdAsync(eventKey);
            return json;
        }

        #endregion // GetAsync



        [AllowAnonymous]
        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<string> PostAsync(Person user)
        {
            EventKey id =  await _eventFlowProducer.Stage1Async(user, "Stage 1 data");
            return id;
        }
    }
}

