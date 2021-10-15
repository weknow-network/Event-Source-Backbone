using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.UnitTests.Entities;

namespace Weknow.EventSource.Backbone
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
