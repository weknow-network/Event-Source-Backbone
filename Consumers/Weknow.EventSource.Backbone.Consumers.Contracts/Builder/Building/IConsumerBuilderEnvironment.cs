namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IConsumerBuilderEnvironment<T>
    {
        /// <summary>
        /// Include the environment as prefix of the stream key.
        /// for example: production:partition-name:shard-name
        /// </summary>
        /// <param name="environment">The environment (null: keep current environment, empty: reset the environment to nothing).</param>
        /// <returns></returns>
        T Environment(string? environment);
    }
}
