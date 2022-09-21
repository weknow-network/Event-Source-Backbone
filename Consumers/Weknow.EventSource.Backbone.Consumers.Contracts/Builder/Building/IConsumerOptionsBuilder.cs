using System;

namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Enable configuration.
    /// </summary>
    public interface IConsumerOptionsBuilder
        : IConsumerHooksBuilder,
         IConsumerOriginFilterBuilder<IConsumerOptionsBuilder>
    {
        /// <summary>
        /// Tune configuration.
        /// </summary>
        /// <param name="optionsStrategy">The options strategy.</param>
        /// <returns></returns>
        IConsumerHooksBuilder WithOptions(Func<ConsumerOptions, ConsumerOptions> optionsStrategy);
    }
}
