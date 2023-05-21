namespace EventSource.Backbone.Building
{

    /// <summary>
    /// Partition key represent logical group
    /// </summary>
    public interface IProducerPartitionBuilder : IProducerRawBuilder, IProducerLoggerBuilder<IProducerPartitionBuilder>
    {
        /// <summary>
        /// The stream key
        /// </summary>
        /// <param name="uri">
        /// The stream identifier (the URI combined with the environment separate one stream from another)
        /// </param>
        /// <returns></returns>
        IProducerHooksBuilder Uri(string uri);
    }
}
