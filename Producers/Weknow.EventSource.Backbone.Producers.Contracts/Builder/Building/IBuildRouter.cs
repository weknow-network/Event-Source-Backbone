using System;

namespace Weknow.EventSource.Backbone
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
        T Build(Func<IProducerPlanRoute, (string? partition, string? shard)> routeStrategy);

        /// <summary>
        /// Enable partition assignment.
        /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        T Build(string partition, RouteAssignmentType type = RouteAssignmentType.Prefix);

        /// <summary>
        /// Enable partition assignment.
        /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="shard">The shard.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        T Build(string partition, string shard, RouteAssignmentType type = RouteAssignmentType.Prefix);
    }
}
