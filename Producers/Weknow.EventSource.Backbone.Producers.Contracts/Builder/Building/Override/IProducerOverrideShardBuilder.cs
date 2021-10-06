namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Enable dynamic transformation of the stream id before sending.
    /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
    /// </summary>
    public interface IProducerOverrideShardBuilder<T> : IProducerOverrideBuildBuilder<T> where T : class
    {
        /// <summary>
        /// Override the shard.
        /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        IProducerOverrideBuildBuilder<T> Shard(string shard, RouteAssignmentType type = RouteAssignmentType.Prefix);
    }
}
