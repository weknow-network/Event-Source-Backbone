namespace EventSource.Backbone
{
    /// <summary>
    /// Subscription Bridge convention
    /// </summary>
    public interface ISubscriptionBridge
    {
        // TODO: [bnaya 2021-10] version filtering

        /// <summary>
        /// Bridges to the subscriber implementation.
        /// </summary>
        /// <param name="announcement">The announcement.</param>
        /// <param name="consumerBridge">The consumer bridge.</param>
        /// <returns>
        /// Indication whether the event had been processed or avoided.
        /// </returns>
        Task<bool> BridgeAsync(Announcement announcement, IConsumerBridge consumerBridge);
    }
}
