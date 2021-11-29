namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IConsumerShardBuilder:
        IConsumerReadyBuilder, IConsumerShardOfBuilder<IConsumerReadyBuilder>
    {
    }
}
