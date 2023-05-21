using System.Text.Json;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StackExchange.Redis;

namespace EventSource.Backbone.WebEventTest.Controllers
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
            IConsumerHooksBuilder baseBuilder,
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
        /// Get event by id
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
        /// Get event by id
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
        /// Get event by id
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpGet("more/{partition}/{shard}/{eventKey}/{env?}")]
        //[AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async ValueTask<JsonElement> GetMoreAsync(string uri, string eventKey, string? env = null)
        {
            var receiver = _baseBuilder.Uri(uri).Environment(env ?? string.Empty).BuildReceiver();
            var json = await receiver.GetJsonByIdAsync(eventKey);
            return json;
        }

        #endregion // GetAsync



        /// <summary>
        /// Produce event
        /// 
        /// Setup environment:
        ///   docker run -p 6379:6379 -it --rm --name redis-Json redislabs/rejson:latest
        ///   docker run --rm -it --name jaeger -p 13133:13133 -p 16686:16686 -p 4317:55680 jaegertracing/opentelemetry-all-in-one
        ///   
        ///   Check Jaeger on: http://localhost:16686/search
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<string> PostAsync(Person user)
        {
            EventKey id = await _eventFlowProducer.Stage1Async(user, "Stage 1 data");
            return id;
        }
    }
}

