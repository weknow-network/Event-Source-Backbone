namespace EventSourcing.Backbone.Building
{

    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConsumerUriBuilder<out T>
    {
        /// <summary>
        /// The stream identifier (the URI combined with the environment separate one stream from another)
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        T Uri(string uri);
    }
}
