namespace EventSourcing.Backbone;


public partial class ConsumerBase
{
    /// <summary>
    /// Fallback handle
    /// </summary>
    [Obsolete("deprecated", true)]
    internal sealed class FallbackHandle : IConsumerFallback
    {
        private readonly Announcement _announcement;
        private readonly IConsumerBridge _consumerBridge;
        private readonly IAck _ack;

        #region Ctor

        public FallbackHandle(
            Announcement announcement,
            IConsumerBridge consumerBridge,
            IAck ack)
        {
            _announcement = announcement;
            Metadata = announcement.Metadata;
            _consumerBridge = consumerBridge;
            _ack = ack;
        }

        #endregion // Ctor

        /// <summary>
        /// Gets the metadata.
        /// </summary>
        public Metadata Metadata { get; }

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
        ValueTask<TParam> IConsumerFallback.GetParameterAsync<TParam>(string argumentName)
        {
            var result = _consumerBridge.GetParameterAsync<TParam>(_announcement, argumentName);
            return result;
        }
    }
}
