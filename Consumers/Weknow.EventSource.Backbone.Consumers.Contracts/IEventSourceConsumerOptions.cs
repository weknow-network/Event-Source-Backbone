namespace Weknow.EventSource.Backbone
{
    public interface IEventSourceConsumerOptions: IEventSourceOptions
    {

        /// <summary>
        /// Gets the max batch size of reading messages per shard.
        /// The framework won't proceed to the next batch until all messages
        /// in the batch complete (or timeout when it set to acknowledge on timeout).
        /// </summary>
        int BatchSize { get; }

        /// <summary>
        /// Gets the acknowledge behavior.
        /// </summary>
        AckBehavior AckBehavior { get; }

        /// <summary>
        /// Gets the maximum messages to consume before detaching the subscription.
        /// any number > 0 will activate this mechanism.
        /// 
        /// Will be evaluate against consuming attempt (either successful or faulted), not includes Polly retry policy
        /// </summary>
        uint MaxMessages { get; }

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
        bool KeepAlive { get; }
    }
}