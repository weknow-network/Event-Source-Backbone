using System;
using System.Collections.Immutable;
using System.Threading;

using Microsoft.Extensions.Logging;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{

    /// <summary>
    /// Plan contract
    /// </summary>
    /// <seealso cref="Weknow.EventSource.Backbone.IProducerPlanBase" />
    public interface IProducerPlan: IProducerPlanBase
    {
        /// <summary>
        /// Gets the communication channel provider.
        /// </summary>
        IProducerChannelProvider Channel { get; }

        /// <summary>
        /// Gets the forwards pipelines.
        /// Result of merging multiple channels.
        /// </summary>
        IImmutableList<IProducerPlan> ForwardPlans { get; }
    }
}