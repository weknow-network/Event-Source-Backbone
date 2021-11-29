namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Enable dynamic transformation of the stream id before sending.
    /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
    /// </summary>
    public interface IProducerOverrideEnvironmentBuilder<T> : IProducerOverridePartitionBuilder<T> where T : class
    {

        /// <summary>
        /// Override the environment.
        /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <returns></returns>
        IProducerOverridePartitionBuilder<T> Environment(string environment);
    }
}
