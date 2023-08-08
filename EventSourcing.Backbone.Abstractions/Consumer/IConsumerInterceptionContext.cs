using Microsoft.Extensions.Logging;

namespace EventSourcing.Backbone;

/// <summary>
/// Responsible for fallback scenario when the message wasn't consumed
/// </summary>
public interface IConsumerInterceptionContext : IAckOperations
{
    ///// <summary>
    ///// Gets the plan.
    ///// </summary>
    //IPlanBase Plan { get; }

    /// <summary>
    /// Gets the logger.
    /// </summary>
    ILogger Logger { get; }

    /// <summary>
    /// Gets the metadata.
    /// </summary>
    ConsumerContext Context { get; }

    /// <summary>
    /// Gets the parameter value from the message.
    /// </summary>
    /// <typeparam name="TParam">The type of the parameter.</typeparam>
    /// <param name="argumentName">Name of the argument.</param>
    /// <returns></returns>
    ValueTask<TParam> GetParameterAsync<TParam>(string argumentName);
}

