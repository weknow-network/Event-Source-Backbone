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
    /// Behavior of delay when empty
    /// </summary>
    public class DelayWhenEmptyBehavior
    {
        public static readonly DelayWhenEmptyBehavior Default = new DelayWhenEmptyBehavior();

        /// <summary>
        /// Gets or sets the maximum delay.
        /// </summary>
        public TimeSpan MaxDelay { get; init; } = TimeSpan.FromSeconds(5);


        /// <summary>
        /// Gets or sets the next delay.
        /// </summary>
        public Func<TimeSpan, TimeSpan> CalcNextDelay { get; init; } = DefaultCalcNextDeay;

        private static TimeSpan DefaultCalcNextDeay(TimeSpan previous)
        {
            var prevMilli = previous.TotalMilliseconds;
            var milli = Max(prevMilli * 2, 10);
            return TimeSpan.FromMilliseconds(milli);
        }
    }
}