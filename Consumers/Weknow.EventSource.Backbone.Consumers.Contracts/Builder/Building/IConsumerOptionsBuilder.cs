namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Enable configuration.
    /// </summary>
    public interface IConsumerOptionsBuilder
        : IConsumerHooksBuilder
    {
        /// <summary>
        /// Attach configuration.
        /// </summary>
        IConsumerHooksBuilder WithOptions(IEventSourceConsumerOptions options);
    }
}
