using Microsoft.Extensions.Logging;

using Polly;
using Polly.Registry;

using StackExchange.Redis;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Private;

using static System.Math;


namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    /// <summary>
    /// Represent specific setting of the consumer channel
    /// </summary>
    public class RedisConsumerChannelSetting
    {
        public static readonly RedisConsumerChannelSetting Empty = new RedisConsumerChannelSetting();

        /// <summary>
        /// Define when to claim stale (long waiting) messages from other consumers
        /// </summary>
        public StaleMessagesClaimingTrigger ClaimingTrigger { get; set; } = StaleMessagesClaimingTrigger.Default;


        /// <summary>
        /// Gets or sets the resilience policy.
        /// </summary>
        public ResiliencePolicies Policy { get; set; } = new ResiliencePolicies();

        public Action<ConfigurationOptions>? RedisConfiguration { get; set; }
    }
}