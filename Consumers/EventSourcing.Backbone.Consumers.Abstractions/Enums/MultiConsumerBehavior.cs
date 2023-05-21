namespace EventSourcing.Backbone.Enums
{
    /// <summary>
    /// Collaborate behavior of multi consumers registered via common subscription object.
    /// </summary>
    /// <example>
    /// <![CDATA[
    /// await using IConsumerLifetime subscription = _consumerBuilder
    ///           .Environment(ENV)
    ///           .Partition(PARTITION)
    ///           .Shard(SHARD)
    ///           .Group("CONSUMER_GROUP_X_1")
    ///           .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
    ///           .SubscribeFlowAConsumer(_subscriberA)
    ///           .SubscribeFlowBConsumer(_subscriberB)
    ///           .SubscribeFlowABConsumer(_subscriberAB);
    /// ]]>
    /// </example>
    public enum MultiConsumerBehavior
    {
        /// <summary>
        /// Only single consumer should successfully consume the message.
        /// If it succeed the message won't be offered to other consumers.
        /// </summary>
        Once,
        /// <summary>
        /// All consumers handle the message in parallel.
        /// If one or more succeed to consume it, it consider as succeed consumption.
        /// </summary>
        All,
    }
}