namespace EventSourcing.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IConsumerEnvironmentBuilder :
        IConsumerUriBuilder<IConsumerReadyBuilder>,
        IConsumerEnvironmentOfBuilder<IConsumerUriBuilder<IConsumerReadyBuilder>>
    {
    }
}
