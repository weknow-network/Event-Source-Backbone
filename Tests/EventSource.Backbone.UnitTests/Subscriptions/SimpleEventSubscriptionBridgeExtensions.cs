using EventSource.Backbone.Building;
using EventSource.Backbone.UnitTests.Entities;

namespace EventSource.Backbone
{
    public static class SimpleEventSubscriptionBridgeExtensions
    {
        public static IConsumerLifetime SubscribeSimpleEvent(
            this IConsumerSubscribeBuilder source,
            ISimpleEventConsumer target)
        {
            var bridge = new SimpleEventSubscriptionBridge(target);
            return source.Subscribe(bridge);
        }
    }
}
