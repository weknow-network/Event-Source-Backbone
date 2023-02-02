using System.Collections.Immutable;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{

    /// <summary>
    /// Consumer contract
    /// </summary>
    public interface IConsumer
    {
        /// <summary>
        /// Gets a consumer plan.
        /// </summary>
        IConsumerPlan Plan { get; }
    }
}