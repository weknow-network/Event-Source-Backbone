using System;
using System.Threading.Tasks.Dataflow;

namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IConsumerBuilder
    {
        /// <summary>
        /// Subscribe consumer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factory">The factory.</param>
        /// <returns>
        /// Un-subscribe from the partition
        /// </returns>
        IAsyncDisposable Subscribe<T>(
            Func<ShardMetadata, T> factory);
    }
}
