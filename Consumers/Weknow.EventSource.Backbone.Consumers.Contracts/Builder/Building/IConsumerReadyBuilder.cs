using Microsoft.Extensions.Logging;

using Polly;

using Weknow.EventSource.Backbone;
using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IConsumerReadyBuilder: IConsumerSubscribeBuilder
    {
        /// <summary>
        /// Attach logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        IConsumerSubscribeBuilder WithLogger(ILogger logger);

        /// <summary>
        /// Set resilience policy
        /// </summary>
        /// <param name="policy">The policy.</param>
        /// <returns></returns>
        IConsumerReadyBuilder WithResiliencePolicy(AsyncPolicy policy);
    }
}
