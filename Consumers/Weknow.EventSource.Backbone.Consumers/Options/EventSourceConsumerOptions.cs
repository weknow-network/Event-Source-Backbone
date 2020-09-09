namespace Weknow.EventSource.Backbone
{
    public class EventSourceConsumerOptions: 
        EventSourceOptions, IEventSourceConsumerOptions
    {
        public new static readonly EventSourceConsumerOptions Empty = new EventSourceConsumerOptions();

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="batchSize">Gets the max batch size of reading messages per shard.
        /// The framework won't proceed to the next batch until all messages
        /// in the batch complete (or timeout when it set to acknowledge on timeout).</param>
        /// <param name="ackBehavior">The acknowledge behavior.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="keepAlive">Gets a value indicating whether to prevent the consumer
        /// from being collect by the GC.
        /// True by default, when you hold the subscription disposable
        /// you can set it to false. as long as you keeping the disposable in
        /// object that isn't candidate for being collected the consumer will stay alive.</param>
        public EventSourceConsumerOptions(
            int batchSize = 100,
            AckBehavior ackBehavior = AckBehavior.Manual,
            IDataSerializer? serializer = null,
            bool keepAlive = true)
            : base(serializer)
        {
            BatchSize = batchSize;
            KeepAlive = keepAlive;
            AckBehavior = ackBehavior;
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
    }
}