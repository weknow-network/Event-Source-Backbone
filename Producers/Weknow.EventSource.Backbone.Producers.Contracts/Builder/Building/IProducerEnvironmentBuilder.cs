namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Origin environment of the message
    /// </summary>
    public interface IProducerEnvironmentBuilder : IProducerPartitionBuilder
    {
        /// <summary>
        /// Origin environment of the message
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <returns></returns>
        IProducerPartitionBuilder Environment(string environment);
    }
}
