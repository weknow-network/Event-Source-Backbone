namespace EventSourcing.Backbone.Building
{

    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IConsumerPartitionBuilder<T>
    {
        /// <summary>
        /// The stream identifier (the URI combined with the environment separate one stream from another)
        /// </summary>
        /// <param name="partition">The partition key.</param>
        /// <returns></returns>
        T Uri(string partition);
    }
}
