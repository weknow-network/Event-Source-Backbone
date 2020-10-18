namespace Weknow.EventSource.Backbone
{
    public class ConsumerOptions: 
        EventSourceOptions, IEventSourceConsumerOptions
    {
        public new static readonly ConsumerOptions Empty = new ConsumerOptions();

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="ackBehavior">The acknowledge behavior.</param>
        /// <param name="batchSize">Gets the max batch size of reading messages per shard.
        /// The framework won't proceed to the next batch until all messages
        /// in the batch complete (or timeout when it set to acknowledge on timeout).</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="maxMessages">
        /// Maximum messages to consume before detaching the subscription.
        /// any number > 0 will activate this mechanism.
        /// </param>
        /// <param name="keepAlive">Gets a value indicating whether to prevent the consumer
        /// from being collect by the GC.
        /// True by default, when you hold the subscription disposable
        /// you can set it to false. as long as you keeping the disposable in
        /// object that isn't candidate for being collected the consumer will stay alive.</param>
        public ConsumerOptions(
            AckBehavior ackBehavior = AckBehavior.Manual,
            int batchSize = 100,
            IDataSerializer? serializer = null,
            uint maxMessages = 0,
            bool keepAlive = true)
            : base(serializer)
        {
            BatchSize = batchSize;
            KeepAlive = keepAlive;
            AckBehavior = ackBehavior;
            MaxMessages = maxMessages;
        }

        #endregion // Ctor

        #region BatchSize

        /// <summary>
        /// Gets the max batch size of reading messages per shard.
        /// The framework won't proceed to the next batch until all messages
        /// in the batch complete (or timeout when it set to acknowledge on timeout).
        /// </summary>
        public int BatchSize { get; }

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
        public bool KeepAlive { get; }

        #endregion // Validation

        #region AckBehavior

        /// <summary>
        /// Gets the acknowledge behavior.
        /// </summary>
        public AckBehavior AckBehavior { get; }

        #endregion AckBehavior 

        #region MaxMessages

        /// <summary>
        /// Gets the maximum messages to consume before detaching the subscription.
        /// any number > 0 will activate this mechanism.
        /// </summary>
        public uint MaxMessages { get; }

        #endregion MaxMessages 
    }
}