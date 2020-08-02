using Microsoft.Extensions.Logging;

namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IProducerLoggerBuilder: IProducerSpecializeBuilder
    {
        /// <summary>
        /// Attach logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        IProducerSpecializeBuilder WithLogger(ILogger logger);
    }
}
