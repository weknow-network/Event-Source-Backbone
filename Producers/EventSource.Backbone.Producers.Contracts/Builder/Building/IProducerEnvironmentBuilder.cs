namespace EventSource.Backbone.Building
{
    /// <summary>
    /// Origin environment of the message
    /// </summary>
    public interface IProducerEnvironmentBuilder : IProducerPartitionBuilder, IProducerBuilderEnvironment<IProducerPartitionBuilder>,
        IProducerRawBuilder
    {
    }
}
