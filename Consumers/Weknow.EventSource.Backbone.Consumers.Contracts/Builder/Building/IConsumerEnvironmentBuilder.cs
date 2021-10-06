namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IConsumerEnvironmentBuilder : IConsumerPartitionBuilder
    {
        /// <summary>
        /// Include the environment as prefix of the stream key.
        /// for example: production:partition-name:shard-name
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <returns></returns>
        IConsumerPartitionBuilder Environment(string environment);
    }
}
