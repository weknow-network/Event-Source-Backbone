namespace EventSource.Backbone.Building
{
    /// <summary>
    /// Enable dynamic transformation of the stream id before sending.
    /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
    /// </summary>
    public interface IProducerOverrideBuildBuilder<T> where T : class
    {
        /// <summary>
        /// <![CDATA[ Ceate Producer proxy for specific events sequence.
        /// Event sequence define by an interface which declare the
        /// operations which in time will be serialized into event's
        /// messages.
        /// This interface can be use as a proxy in the producer side,
        /// and as adapter on the consumer side.
        /// All method of the interface should represent one-way communication pattern
        /// and return Task or ValueTask (not Task<T> or ValueTask<T>).
        /// Nothing but method allowed on this interface]]>
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <returns></returns>
        T Build(Func<IProducerPlan, T> factory);
    }
}
