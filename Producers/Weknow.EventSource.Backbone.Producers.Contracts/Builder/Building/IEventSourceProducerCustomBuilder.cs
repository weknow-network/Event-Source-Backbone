using System;

namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IEventSourceProducerCustomBuilder
        : IEventSourceProducer2Builder
    {
        /// <summary>
        /// Custom channel will replace the default channel.
        /// It should be used carefully for isolated domain,
        /// Make sure the data sequence don't have to be synchronize with other channels.
        /// </summary>
        /// <param name="name">The channel name.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IEventSourceProducer2Builder ChangeChannel(
                                    string name);
    }
}
