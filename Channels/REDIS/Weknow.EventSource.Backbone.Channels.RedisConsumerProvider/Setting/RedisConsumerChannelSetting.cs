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
    public record RedisConsumerChannelSetting
    {
        public static readonly RedisConsumerChannelSetting Default = new RedisConsumerChannelSetting();

        /// <summary>
        /// Define when to claim stale (long waiting) messages from other consumers
        /// </summary>
        public StaleMessagesClaimingTrigger ClaimingTrigger { get; set; } = StaleMessagesClaimingTrigger.Default;


        /// <summary>
        /// Gets or sets the resilience policy.
        /// </summary>
        public ResiliencePolicies Policy { get; set; } = new ResiliencePolicies();


        /// <summary>
        /// Behavior of delay when empty
        /// </summary>
        public DelayWhenEmptyBehavior DelayWhenEmptyBehavior { get; set; } = DelayWhenEmptyBehavior.Default;

        #region Cast overloads

        /// <summary>
        /// Performs an implicit conversion.
        /// </summary>
        /// <param name="policy">The policy.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator RedisConsumerChannelSetting(AsyncPolicy? policy)
        {
            if (policy == null) return Default;

            return Default with { Policy = policy };
        }

        #endregion // Cast overloads
    }
}