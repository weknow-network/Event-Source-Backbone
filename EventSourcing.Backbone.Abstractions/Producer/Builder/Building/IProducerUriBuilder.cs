namespace EventSourcing.Backbone.Building
{

    /// <summary>
    /// The stream's key (identity)
    /// </summary>
    public interface IProducerUriBuilder : IProducerRawBuilder, IProducerLoggerBuilder<IProducerUriBuilder>
    {
        /// <summary>
        /// The stream key
        /// </summary>
        /// <param name="uri">
        /// The stream identifier (the URI combined with the environment separate one stream from another)
        /// </param>
        /// <returns></returns>
        IProducerHooksBuilder Uri(string uri);
    }
}
