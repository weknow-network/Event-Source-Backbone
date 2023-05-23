namespace EventSourcing.Backbone.Building
{
    /// <summary>
    /// Origin environment of the message
    /// </summary>
    public interface IProducerEnvironmentBuilder : IProducerUriBuilder, IProducerBuilderEnvironment<IProducerUriBuilder>,
        IProducerRawBuilder
    {
    }
}
