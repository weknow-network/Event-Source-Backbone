﻿
using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Enable configuration.
    /// </summary>
    public interface IEventSourceProducerShardBuilder
    {
        /// <summary>
        /// Shard key represent physical sequence.
        /// Use same shard when order is matter.
        /// For example: assuming each ORDERING flow can have its 
        /// own messaging sequence, in this case you can split each 
        /// ORDER into different shard and gain performance bust..
        /// </summary>
        /// <param name="shardKey">The shard key.</param>
        /// <returns></returns>
        IEventSourceProducerHooksBuilder Shard(string shardKey);
    }
}