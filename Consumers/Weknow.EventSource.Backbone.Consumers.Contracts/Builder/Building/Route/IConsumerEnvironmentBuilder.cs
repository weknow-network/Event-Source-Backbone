namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IConsumerEnvironmentBuilder : 
        IConsumerPartitionBuilder<IConsumerShardBuilder>, 
        IConsumerEnvironmentOfBuilder<IConsumerPartitionBuilder<IConsumerShardBuilder>>
    {
    }
}
