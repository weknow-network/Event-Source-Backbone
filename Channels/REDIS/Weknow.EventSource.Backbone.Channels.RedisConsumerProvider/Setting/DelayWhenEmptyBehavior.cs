using static System.Math;


namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    /// <summary>
    /// Behavior of delay when empty
    /// </summary>
    public record DelayWhenEmptyBehavior
    {
        public static readonly DelayWhenEmptyBehavior Default = new DelayWhenEmptyBehavior();

        /// <summary>
        /// Gets or sets the maximum delay.
        /// </summary>
        public TimeSpan MaxDelay { get; init; } = TimeSpan.FromSeconds(5);


        /// <summary>
        /// Gets or sets the next delay.
        /// </summary>
        public Func<TimeSpan, TimeSpan> CalcNextDelay { get; init; } = DefaultCalcNextDelay;

        /// <summary>
        /// Default calculation of next delay.
        /// </summary>
        /// <param name="previous">The previous delay.</param>
        /// <returns></returns>
        private static TimeSpan DefaultCalcNextDelay(TimeSpan previous)
        {
            var prevMilli = previous.TotalMilliseconds;
            var milli = Max(prevMilli * 2, 10);
            return TimeSpan.FromMilliseconds(milli);
        }
    }
}