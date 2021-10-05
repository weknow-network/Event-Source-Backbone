using System;

namespace Weknow.EventSource.Backbone.Building
{

    /// <summary>
    /// Enable dynamic transformation of the stream id before sending.
    /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
    /// </summary>
    public interface IBuildRouter<T> where T : class
    {
        /// <summary>
        /// Enable dynamic transformation of the stream id before sending.
        /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
        /// </summary>
        /// <param name="routeStrategy">The routing strategy.</param>
        /// <returns></returns>
        T Build(Func<IProducerPlanRoute, IProducerPlanRoute> routeStrategy);
    }
}
