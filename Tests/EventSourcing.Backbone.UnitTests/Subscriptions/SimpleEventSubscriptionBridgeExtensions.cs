using EventSourcing.Backbone.Building;
using EventSourcing.Backbone.UnitTests.Entities;

namespace EventSourcing.Backbone
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
