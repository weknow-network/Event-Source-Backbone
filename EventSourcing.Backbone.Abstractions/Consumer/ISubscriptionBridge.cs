namespace EventSourcing.Backbone;

/// <summary>
/// Subscription Bridge convention
/// </summary>
public interface ISubscriptionBridge<out T> : ISubscriptionBridge
    where T : class
{
    /// <summary>
    /// Gets the target consumer.
    /// Useful for fallback (handling deprecated / up versions messages) 
    /// </summary>
    T Consumer { get; }
}

/// <summary>
/// Subscription Bridge convention
/// </summary>
public interface ISubscriptionBridge
{
    /// <summary>
    /// Bridges to the subscriber implementation.
    /// </summary>
    /// <param name="announcement">The announcement.</param>
    /// <param name="consumerBridge">The consumer bridge.</param>
    /// <param name="plan">The plan.</param>
    /// <returns>
    /// Indication whether the event had been processed or avoided.
    /// </returns>
    Task<bool> BridgeAsync(Announcement announcement, IConsumerBridge consumerBridge, IPlanBase plan);
}
