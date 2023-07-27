﻿using EventSourcing.Backbone.UnitTests.Entities;

namespace EventSourcing.Backbone
{

    /// <summary>
    /// In-Memory Channel (excellent for testing)
    /// </summary>
    /// <seealso cref="EventSourcing.Backbone.IProducerChannelProvider" />
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

        async Task<bool> ISubscriptionBridge.BridgeAsync(Announcement announcement, IConsumerBridge consumerBridge)
        {
            var meta = ConsumerMetadata.Context;
            switch (announcement.Metadata)
            {
                case { Operation: nameof(ISimpleEventConsumer.ExecuteAsync) }:
                    {
                        var p0 = await consumerBridge.GetParameterAsync<string>(announcement, "key");
                        var p1 = await consumerBridge.GetParameterAsync<int>(announcement, "value");
                        await _target.ExecuteAsync(meta, p0, p1);
                        return true;
                    }
                case { Operation: nameof(ISimpleEventConsumer.RunAsync), ParamsSignature: "Int32,DateTime" }:
                    {
                        var p0 = await consumerBridge.GetParameterAsync<int>(announcement, "id");
                        var p1 = await consumerBridge.GetParameterAsync<DateTime>(announcement, "date");
                        await _target.RunAsync(meta, p0, p1);
                        return true;
                    }
                default:
                    break;

            }
            return false;
        }
    }
}
