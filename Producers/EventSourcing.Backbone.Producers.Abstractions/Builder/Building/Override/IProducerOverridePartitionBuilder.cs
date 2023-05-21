namespace EventSourcing.Backbone.Building
{
    /// <summary>
    /// Enable dynamic transformation of the stream id before sending.
    /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
    /// </summary>
    public interface IProducerOverridePartitionBuilder<T> : IProducerOverrideBuildBuilder<T> where T : class
    {

        /// <summary>
        /// Override the partition.
        /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        IProducerOverrideBuildBuilder<T> Partition(string partition, RouteAssignmentType type = RouteAssignmentType.Prefix);
    }
}
