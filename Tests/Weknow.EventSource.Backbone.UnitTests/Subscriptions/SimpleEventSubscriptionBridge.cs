﻿using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.UnitTests.Entities;

namespace Weknow.EventSource.Backbone
{

    /// <summary>
    /// In-Memory Channel (excellent for testing)
    /// </summary>
    /// <seealso cref="Weknow.EventSource.Backbone.IProducerChannelProvider" />
    public class SimpleEventSubscriptionBridge : ISubscriptionBridge
    {
        private readonly ISimpleEventConsumer _target;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target">The target.</param>
        public SimpleEventSubscriptionBridge(ISimpleEventConsumer target)
        {
            _target = target;
        }

        #endregion // Ctor

        async Task ISubscriptionBridge.BridgeAsync(Announcement announcement, IConsumerBridge consumerBridge)
        {
            switch (announcement.Metadata.Operation)
            {
                case nameof(ISimpleEventConsumer.ExecuteAsync):
                    {
                        var p0 = await consumerBridge.GetParameterAsync<string>(announcement, "key");
                        var p1 = await consumerBridge.GetParameterAsync<int>(announcement, "value");
                        await _target.ExecuteAsync(p0, p1);
                        break;
                    }
                case nameof(ISimpleEventConsumer.RunAsync):
                    {
                        var p0 = await consumerBridge.GetParameterAsync<int>(announcement, "id");
                        var p1 = await consumerBridge.GetParameterAsync<DateTime>(announcement, "date");
                        await _target.RunAsync(p0, p1);
                        break;
                    }
                default:
                    break;
            }
        }
    }
}