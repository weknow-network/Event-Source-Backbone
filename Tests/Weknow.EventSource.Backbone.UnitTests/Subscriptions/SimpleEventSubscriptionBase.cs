using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.UnitTests.Entities;

namespace Weknow.EventSource.Backbone
{

    /// <summary>
    /// Base class for subscription of ISimpleEventConsumer
    /// </summary>
    /// <seealso cref="ISimpleEventConsumer" />
    public abstract class SimpleEventSubscriptionBase : ISubscriptionBridge
    {

        async Task ISubscriptionBridge.BridgeAsync(Announcement announcement, IConsumerBridge consumerBridge)
        {
            switch (announcement.Metadata.Operation)
            {
                case nameof(ISimpleEventConsumer.ExecuteAsync):
                    {
                        var p0 = await consumerBridge.GetParameterAsync<string>(announcement, "key");
                        var p1 = await consumerBridge.GetParameterAsync<int>(announcement, "value");
                        await ExecuteAsync(p0, p1);
                        break;
                    }
                case nameof(ISimpleEventConsumer.RunAsync):
                    {
                        var p0 = await consumerBridge.GetParameterAsync<int>(announcement, "id");
                        var p1 = await consumerBridge.GetParameterAsync<DateTime>(announcement, "date");
                        await RunAsync(p0, p1);
                        break;
                    }
                default:
                    break;
            }
        }

        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected abstract ValueTask ExecuteAsync(string key, int value);

        /// <summary>
        /// Runs the asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        protected abstract ValueTask RunAsync(int id, DateTime date);
    }
}
