namespace EventSourcing.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IConsumerEnvironmentBuilder :
        IConsumerPartitionBuilder<IConsumerReadyBuilder>,
        IConsumerEnvironmentOfBuilder<IConsumerPartitionBuilder<IConsumerReadyBuilder>>
    {
    }
}
