using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using StackExchange.Redis;

namespace Weknow.EventSource.Backbone.WebEventTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IEventSourceRedisConnectionFacroty _connFacroty;

        public TestController(
            ILogger<EventSourceApiController> logger,
            IEventSourceRedisConnectionFacroty connFacroty)
        {
            _logger = logger;
            _connFacroty = connFacroty;
        }

        /// <summary>
        /// Post Analysts
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpGet("conn")]
        //[AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async ValueTask<string> GetAsync()
        {
            var conn = await RedisClientFactory.CreateProviderAsync(_logger);
            var status = conn.GetStatus();
            var db = conn.GetDatabase();
            var p = await db.PingAsync();

            return @$"ClientName: {conn.ClientName}, IsConnected: {conn.IsConnected},
timeout (ms): {conn.TimeoutMilliseconds},
status: {status}, ping: {p}";
        }

        /// <summary>
        /// Post Analysts
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpGet("ping")]
        //[AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async ValueTask<TimeSpan> GetPingAsync()
        {
            IDatabaseAsync db = await _connFacroty.GetDatabaseAsync();
            var p = await db.PingAsync();
            return p;
        }

        /// <summary>
        /// Post Analysts
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpGet("static")]
        //[AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async ValueTask<string> GetStaticAsync()
        {
            IDatabaseAsync db = await _connFacroty.GetDatabaseAsync();
            var p = await db.PingAsync();

            return @$"ping: {p}";
        }
    }
}

