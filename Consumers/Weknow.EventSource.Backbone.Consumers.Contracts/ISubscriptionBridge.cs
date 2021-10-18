﻿using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
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
        /// <returns></returns>
        Task BridgeAsync(Announcement announcement, IConsumerBridge consumerBridge);
    }
}