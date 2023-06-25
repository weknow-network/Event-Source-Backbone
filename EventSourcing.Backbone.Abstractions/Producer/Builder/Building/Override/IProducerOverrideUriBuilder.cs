namespace EventSourcing.Backbone.Building
{
    /// <summary>
    /// Enable dynamic transformation of the stream id before sending.
    /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
    /// </summary>
    public interface IProducerOverrideUriBuilder<T> : IProducerOverrideBuildBuilder<T> where T : class
    {

        /// <summary>
        /// Override the URI.
        /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        IProducerOverrideBuildBuilder<T> Uri(string uri, RouteAssignmentType type = RouteAssignmentType.Prefix);
    }
}
