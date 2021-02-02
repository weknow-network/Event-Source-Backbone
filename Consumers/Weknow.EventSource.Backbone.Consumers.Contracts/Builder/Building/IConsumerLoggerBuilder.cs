using Microsoft.Extensions.Logging;

using Polly;

namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IConsumerLoggerBuilder: IConsumerSubscribeBuilder
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
        IConsumerLoggerBuilder WithResiliencePolicy(AsyncPolicy policy);
    }
}
