using Microsoft.Extensions.Logging;

namespace EventSourcing.Backbone;


/// <summary>
/// Fallback handle
/// </summary>
public sealed class ConsumerInterceptionContext : IConsumerInterceptionContext
{
    private readonly Announcement _announcement;
    private readonly IConsumerBridge _consumerBridge;
    private readonly IPlanBase _plan;
    private readonly IAckOperations _ack;
    private readonly ConsumerContext _context;

    #region Ctor

    public ConsumerInterceptionContext(
        Announcement announcement,
        IConsumerBridge consumerBridge,
        ConsumerContext context,
        IPlanBase plan)
    {
        _announcement = announcement;
        _plan = plan;
        _consumerBridge = consumerBridge;
        _ack = context;
        _context = context;
    }

    #endregion // Ctor

    /// <summary>
    /// Gets the logger.
    /// </summary>
    ILogger IConsumerInterceptionContext.Logger => _plan.Logger;

    /// <summary>
    /// Gets the metadata.
    /// </summary>
    ConsumerContext IConsumerInterceptionContext.Context => _context;

    /// <summary>
    /// Preform acknowledge (which should prevent the
    /// message from process again by the consumer).
    /// Must be execute from a consuming scope (i.e. method call invoked by the consumer's event processing)
    /// </summary>
    /// <param name="cause">The cause of the acknowledge.</param>
    /// <returns></returns>
    ValueTask IAckOperations.AckAsync(AckBehavior cause) => _ack.AckAsync(cause);

    /// <summary>
    /// Cancel acknowledge (will happen on error in order to avoid ack on succeed)
    /// </summary>
    /// <param name="cause">The cause of the cancellation.</param>
    /// <returns></returns>
    /// Must be execute from a consuming scope (i.e. method call invoked by the consumer's event processing)
    ValueTask IAckOperations.CancelAsync(AckBehavior cause) => _ack.CancelAsync(cause);

    /// <summary>
    /// Gets the parameter value from the message.
    /// </summary>
    /// <typeparam name="TParam">The type of the parameter.</typeparam>
    /// <param name="argumentName">Name of the argument.</param>
    /// <returns></returns>
    ValueTask<TParam> IConsumerInterceptionContext.GetParameterAsync<TParam>(string argumentName)
    {
        var result = _consumerBridge.GetParameterAsync<TParam>(_announcement, argumentName);
        return result;
    }
}
