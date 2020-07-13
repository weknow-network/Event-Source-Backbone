
using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IEventSourceProducerChannelBuilder
    {
        /// <summary>
        /// Choose the communication channel provider.
        /// </summary>
        /// <param name="channel">The channel provider.</param>
        /// <returns></returns>
        IEventSourceProducerOptionsBuilder UseChannel(
            IProducerChannelProvider channel);

        /// <summary>
        /// Merges multiple channels of same contract into single
        /// producer for broadcasting messages via all channels.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first">The first channel.</param>
        /// <param name="second">The second channel.</param>
        /// <param name="others">The others channels.</param>
        /// <returns></returns>
        IEventSourceProducerDecoratorBuilder<T> Merge<T>(
            IEventSourceProducerDecoratorBuilder<T> first,
            IEventSourceProducerDecoratorBuilder<T> second,
            params IEventSourceProducerDecoratorBuilder<T>[] others)
            where T: class;
    }
}
