
using System;
using System.Threading;

using Microsoft.Extensions.Logging;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
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
    }
}
