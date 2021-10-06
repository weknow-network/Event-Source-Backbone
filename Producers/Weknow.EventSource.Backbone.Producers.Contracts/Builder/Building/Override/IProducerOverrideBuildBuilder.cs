namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Enable dynamic transformation of the stream id before sending.
    /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
    /// </summary>
    public interface IProducerOverrideBuildBuilder<T> where T : class
    {
        /// <summary>
        /// Builds the producer.
        /// </summary>
        /// <returns></returns>
        T Build();
    }
}
