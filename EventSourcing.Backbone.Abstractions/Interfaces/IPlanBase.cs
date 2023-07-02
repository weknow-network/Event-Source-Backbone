using Microsoft.Extensions.Logging;

namespace EventSourcing.Backbone;

/// <summary>
/// Plan routing identification
/// </summary>
public interface IPlanBase: IPlanRoute
{
    /// <summary>
    /// Gets the configuration.
    /// </summary>
    EventSourceOptions Options { get; }
    /// <summary>
    /// Gets the logger.
    /// </summary>
    ILogger Logger { get; }
}
