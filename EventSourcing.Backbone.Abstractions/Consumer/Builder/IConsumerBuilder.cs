using System;

using EventSourcing.Backbone.Building;

using Microsoft.Extensions.Logging;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Event Source Consumer builder.
    /// </summary>
    public interface IConsumerBuilder
    {
        /// <summary>
        /// Choose the communication channel provider.
        /// </summary>
        /// <param name="channel">The channel provider.</param>
        /// <returns></returns>
        IConsumerStoreStrategyBuilder UseChannel(Func<ILogger, IConsumerChannelProvider> channel);

        /// <summary>
        /// Choose the communication channel provider.
        /// </summary>
        /// <param name="serviceProvider">Dependency injection provider.</param>
        /// <param name="channel">The channel provider.</param>
        /// <returns></returns>
        IConsumerIocStoreStrategyBuilder UseChannel(IServiceProvider serviceProvider, Func<ILogger, IConsumerChannelProvider> channel);
    }
}
