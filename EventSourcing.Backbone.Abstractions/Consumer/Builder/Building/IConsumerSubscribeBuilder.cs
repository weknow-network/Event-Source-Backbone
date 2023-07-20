namespace EventSourcing.Backbone.Building
{

    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IConsumerSubscribeBuilder :
        IConsumerSubscriptionHubBuilder,
        IConsumerEnvironmentOfBuilder<IConsumerSubscribeBuilder>,
        IConsumerUriBuilder<IConsumerSubscribeBuilder>,
        IWithCancellation<IConsumerSubscribeBuilder>
    //IConsumerShardOfBuilder<IConsumerSubscribeBuilder>
    {
        /// <summary>
        /// Tune configuration.
        /// </summary>
        /// <param name="optionsStrategy">The options strategy.</param>
        /// <returns></returns>
        IConsumerSubscribeBuilder WithOptions(Func<ConsumerOptions, ConsumerOptions> optionsStrategy);

        /// <summary>
        /// Consumer's group name.
        /// </summary>
        /// <param name="consumerGroup">Alter the default consumer's group name.</param>
        /// <returns></returns>
        IConsumerSubscribeBuilder Group(string consumerGroup);

        /// <summary>
        /// Set the consumer's name
        /// </summary>
        IConsumerSubscribeBuilder Name(string consumerName);


        /// <summary>
        /// Fallback the specified action.
        /// </summary>
        /// <param name="onFallback">The fallback's action.</param>
        /// <returns></returns>
        IConsumerSubscriptionHubBuilder Fallback(Func<IConsumerFallback, Task> onFallback);


        /// <summary>
        /// The routing information attached to this builder
        /// </summary>
        IPlanRoute Route { get; }

        /// <summary>
        /// Build receiver (on demand data query).
        /// </summary>
        /// <returns></returns>
        IConsumerReceiver BuildReceiver();

        /// <summary>
        /// Build iterator (pull fusion).
        /// </summary>
        /// <returns></returns>
        IConsumerIterator BuildIterator();
    }
}
