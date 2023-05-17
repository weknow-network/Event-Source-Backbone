// TODO: [bnaya 2021-02] use Record

namespace EventSource.Backbone
{
    /// <summary>
    /// Define when to claim stale (long waiting) messages from other consumers
    /// </summary>
    public record ClaimingTrigger
    {
        public static readonly ClaimingTrigger Default = new ClaimingTrigger();

        /// <summary>
        /// Empty batch count define number of empty fetching cycle in a row 
        /// which will trigger operation of trying to get stale messages from other consumers.
        /// </summary>
        public ushort EmptyBatchCount { get; init; } = 1000;

        /// <summary>
        /// The minimum message idle time to allow the reassignment of the message(s).
        /// </summary>
        public TimeSpan MinIdleTime { get; init; } = TimeSpan.FromMinutes(5);

        ///// <summary>
        ///// Define schedule which will trigger 
        ///// operation of trying to get stale messages from other consumers.
        ///// </summary>
        //public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(10);
    }
}