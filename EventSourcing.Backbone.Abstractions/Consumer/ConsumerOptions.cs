using EventSourcing.Backbone.Enums;

namespace EventSourcing.Backbone;

// TODO: [bnaya, 2023-07-09] Consider if batch size, claiming, keep alive aren't a provider specific setting

public record ConsumerOptions :
    EventSourceOptions
{
    #region BatchSize

    /// <summary>
    /// Gets the max batch size of reading messages per shard.
    /// The framework won't proceed to the next batch until all messages
    /// in the batch complete (or timeout when it set to acknowledge on timeout).
    /// </summary>
    public int BatchSize { get; init; } = 100;

    #endregion // BatchSize

    #region KeepAlive

    /// <summary>
    /// Gets a value indicating whether to prevent the consumer
    /// from being collect by the GC.
    /// True by default, when you hold the subscription disposable
    /// you can set it to false. as long as you keeping the disposable in
    /// object that isn't candidate for being collected the consumer will stay alive.
    /// </summary>
    /// <value>
    ///   <c>true</c> if [keep alive]; otherwise, <c>false</c>.
    /// </value>
    public bool KeepAlive { get; init; } = true;

    #endregion // Validation

    #region FetchUntilDateOrEmpty

    /// <summary>
    /// Stop consuming when the stream is empty or reach a specific date.
    /// Great for fetching data until now.
    /// </summary>
    /// <value>
    ///   <c>true</c> if [stop when empty]; otherwise, <c>false</c>.
    /// </value>
    public DateTimeOffset? FetchUntilDateOrEmpty
    {
        get
        {
            var value = FetchUntilUnixDateOrEmpty;
            return (value == null)
                        ? null
                        : DateTimeOffset.FromUnixTimeMilliseconds(value.Value);
        }
        init
        {
            if (value == null)
                FetchUntilUnixDateOrEmpty = null;
            else
                FetchUntilUnixDateOrEmpty = value.Value.ToUnixTimeMilliseconds();
        }
    }

    /// <summary>
    /// Stop consuming when the stream is empty or reach a specific date.
    /// Great for fetching data until now.
    /// </summary>
    /// <value>
    ///   <c>true</c> if [stop when empty]; otherwise, <c>false</c>.
    /// </value>
    public long? FetchUntilUnixDateOrEmpty { get; init; }

    #endregion // FetchUntilDateOrEmpty

    #region AckBehavior

    /// <summary>
    /// Gets the acknowledge behavior.
    /// </summary>
    public AckBehavior AckBehavior { get; init; } = AckBehavior.OnSucceed;

    #endregion AckBehavior 

    #region PartialBehavior

    /// <summary>
    /// Gets or sets the partial behavior 
    ///  - does consumer had to handle all events?.
    ///  - can it ignore event and proceed (marking the event as consumed on its Consumer Group) 
    ///    or should it wait to other consumers to consume the event.
    /// </summary>
    public PartialConsumerBehavior PartialBehavior { get; init; } = PartialConsumerBehavior.Sequential;

    #endregion // PartialBehavior

    #region MultiConsumerBehavior

    /// <summary>
    /// Collaborate behavior of multi consumers registered via common subscription object.
    /// </summary>
    /// <example>
    /// <![CDATA[
    /// await using IConsumerLifetime subscription = _consumerBuilder
    ///           .Environment(ENV)
    ///           .Uri(URI)
    ///           .Group("CONSUMER_GROUP_X_1")
    ///           .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
    ///           .SubscribeFlowAConsumer(_subscriberA)
    ///           .SubscribeFlowBConsumer(_subscriberB)
    ///           .SubscribeFlowABConsumer(_subscriberAB);
    /// ]]>
    /// </example>
    public MultiConsumerBehavior MultiConsumerBehavior { get; init; } = MultiConsumerBehavior.All;

    #endregion // MultiConsumerBehavior

    #region MaxMessages

    /// <summary>
    /// Gets the maximum messages to consume before detaching the subscription.
    /// any number > 0 will activate this mechanism.
    /// </summary>
    public uint MaxMessages { get; init; } = 0;

    #endregion MaxMessages 

    #region OriginFilter

    /// <summary>
    /// Gets the maximum messages to consume before detaching the subscription.
    /// any number > 0 will activate this mechanism.
    /// Make sure to set it to Origin in case you have clone the stream (copy) and you don't want to process copied messages.
    /// </summary>
    public MessageOrigin OriginFilter { get; init; } = MessageOrigin.None;

    #endregion OriginFilter 

    #region ClaimingTrigger

    /// <summary>
    /// Define when to claim stale (long waiting) messages from other consumers
    /// </summary>
    public ClaimingTrigger ClaimingTrigger { get; init; } = ClaimingTrigger.Default;

    #endregion // ClaimingTrigger
}