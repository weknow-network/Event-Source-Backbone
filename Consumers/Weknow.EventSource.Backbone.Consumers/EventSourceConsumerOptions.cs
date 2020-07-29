using System;

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
        /// <param name="batchSize">
        /// Gets the max batch size of reading messages per shard.
        /// The framework won't proceed to the next batch until all messages
        /// in the batch complete (or timeout when it set to acknowledge on timeout).
        /// </param>
        /// <param name="timeoutBehavior">
        /// Define the behavior of the framework on timeout.
        /// </param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="keepAlive">
        /// Gets a value indicating whether to prevent the consumer
        /// from being collect by the GC.
        /// True by default, when you hold the subscription disposable
        /// you can set it to false. as long as you keeping the disposable in
        /// object that isn't candidate for being collected the consumer will stay alive.
        /// </param>
        public EventSourceConsumerOptions(
            int batchSize = 100,
            TimeoutBehavior timeoutBehavior = TimeoutBehavior.Ack,
            IDataSerializer? serializer = null,
            bool keepAlive = true)
            : base(serializer)
        {
            BatchSize = batchSize;
            TimeoutBehavior = timeoutBehavior;
            KeepAlive = keepAlive;
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

        #region TimeoutBehavior

        /// <summary>
        /// Define the behavior of the framework on timeout.
        /// </summary>
        public TimeoutBehavior TimeoutBehavior { get; }

        #endregion // TimeoutBehavior

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
    }
}