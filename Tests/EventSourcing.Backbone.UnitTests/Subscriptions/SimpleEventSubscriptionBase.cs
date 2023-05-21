using EventSourcing.Backbone.UnitTests.Entities;

namespace EventSourcing.Backbone
{

    /// <summary>
    /// Base class for subscription of ISimpleEventConsumer
    /// </summary>
    /// <seealso cref="ISimpleEventConsumer" />
    public abstract class SimpleEventSubscriptionBase : ISubscriptionBridge
    {

        async Task<bool> ISubscriptionBridge.BridgeAsync(Announcement announcement, IConsumerBridge consumerBridge)
        {
            switch (announcement.Metadata.Operation)
            {
                case nameof(ISimpleEventConsumer.ExecuteAsync):
                    {
                        var p0 = await consumerBridge.GetParameterAsync<string>(announcement, "key");
                        var p1 = await consumerBridge.GetParameterAsync<int>(announcement, "value");
                        await ExecuteAsync(p0, p1);
                        return true;
                    }
                case nameof(ISimpleEventConsumer.RunAsync):
                    {
                        var p0 = await consumerBridge.GetParameterAsync<int>(announcement, "id");
                        var p1 = await consumerBridge.GetParameterAsync<DateTime>(announcement, "date");
                        await RunAsync(p0, p1);
                        return true;
                    }
                default:
                    break;
            }
            return false;
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
