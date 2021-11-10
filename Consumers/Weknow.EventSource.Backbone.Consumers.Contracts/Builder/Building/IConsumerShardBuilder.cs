namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IConsumerShardBuilder:
        IConsumerReadyBuilder
    {

        /// <summary>
        /// Shard key represent physical sequence.
        /// On the consumer side shard is optional
        /// for listening on a physical source rather on the entire partition.
        /// Use same shard when order is matter.
        /// For example: assuming each ORDERING flow can have its 
        /// own messaging sequence, in this case you can split each 
        /// ORDER into different shard and gain performance bust..
        /// </summary>
        /// <param name="shardKey">The shard key.</param>
        /// <returns></returns>
        IConsumerReadyBuilder Shard(string shardKey);
    }
}
