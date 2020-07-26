
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
        IConsumerOptionsBuilder UseChannel(IConsumerChannelProvider channel);
    }
}
