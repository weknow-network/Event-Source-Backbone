using Microsoft.Extensions.Logging;

namespace EventSourcing.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IProducerLoggerBuilder<T>
    {
        /// <summary>
        /// Attach logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        T WithLogger(ILogger logger);
    }

    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IProducerLoggerBuilder : IProducerSpecializeBuilder, IProducerLoggerBuilder<IProducerSpecializeBuilder>
    {
    }
}
