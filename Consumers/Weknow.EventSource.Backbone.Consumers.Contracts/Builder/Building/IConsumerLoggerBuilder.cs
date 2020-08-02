using Microsoft.Extensions.Logging;

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
    }
}
