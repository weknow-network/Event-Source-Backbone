using EventSource.Backbone.Building;

namespace EventSource.Backbone
{

    /// <summary>
    /// Enable dynamic transformation of the stream id before sending.
    /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
    /// </summary>
    public interface IProducerOverrideBuilder<T> : IProducerOverrideEnvironmentBuilder<T> where T : class
    {

        /// <summary>
        /// Dynamic override of the stream id before sending.
        /// Can use for scenario like routing between environment like dev vs. prod or AWS vs Azure.
        /// </summary>
        /// <param name="routeStrategy">The routing strategy.</param>
        /// <returns></returns>
        IProducerOverrideBuildBuilder<T> Strategy(Func<IPlanRoute, (string? environment, string? uri)> routeStrategy);
    }
}
