namespace EventSourcing.Backbone;

/// <summary>
/// Responsible for fallback scenario when the message wasn't consumed
/// </summary>
public interface IConsumerFallbackHandle : IAckOperations
{
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

