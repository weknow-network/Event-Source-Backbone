namespace EventSource.Backbone.Building
{
    /// <summary>
    /// Enable configuration.
    /// </summary>
    public interface IConsumerOptionsBuilder
        : IConsumerHooksBuilder
    {
        /// <summary>
        /// Tune configuration.
        /// </summary>
        /// <param name="optionsStrategy">The options strategy.</param>
        /// <returns></returns>
        IConsumerHooksBuilder WithOptions(Func<ConsumerOptions, ConsumerOptions>? optionsStrategy);
    }
}
