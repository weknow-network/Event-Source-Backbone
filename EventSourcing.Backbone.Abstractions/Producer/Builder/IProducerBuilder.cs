using System;

using EventSourcing.Backbone.Building;

using Microsoft.Extensions.Logging;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IProducerBuilder
    {
        /// <summary>
        /// Choose the communication channel provider.
        /// </summary>
        /// <param name="channel">The channel provider.</param>
        /// <returns></returns>
        IProducerStoreStrategyBuilder UseChannel(
            Func<ILogger, IProducerChannelProvider> channel);

        /// <summary>
        /// Choose the communication channel provider.
        /// </summary>
        /// <param name="channel">The channel provider.</param>
        /// <param name="serviceProvider">Dependency injection provider.</param>
        /// <returns></returns>
        IProducerIocStoreStrategyBuilder UseChannel(
            IServiceProvider serviceProvider,
            Func<ILogger, IProducerChannelProvider> channel);

        /// <summary>
        /// Merges multiple channels of same contract into single
        /// producer for broadcasting messages via all channels.
        /// </summary>
        /// <param name="first">The first channel.</param>
        /// <param name="second">The second channel.</param>
        /// <param name="others">The others channels.</param>
        /// <returns></returns>
        IProducerHooksBuilder Merge(
            IProducerHooksBuilder first,
            IProducerHooksBuilder second,
            params IProducerHooksBuilder[] others);
    }
}
