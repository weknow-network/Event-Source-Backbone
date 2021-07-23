using System;
using System.Collections.Immutable;
using System.Threading;

using Microsoft.Extensions.Logging;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Plan builder contract
    /// </summary>
    /// <seealso cref="Weknow.EventSource.Backbone.IProducerPlanBase" />
    public interface IProducerPlanBuilder: IProducerPlanBase
    {
        /// <summary>
        /// Gets the communication channel provider factory.
        /// </summary>
        Func<ILogger, IProducerChannelProvider> ChannelFactory { get; }

        /// <summary>
        /// Gets the forwards pipelines.
        /// Result of merging multiple channels.
        /// </summary>
        IImmutableList<IProducerHooksBuilder> Forwards { get; }

        /// <summary>
        /// Buildings the plan.
        /// </summary>
        /// <returns></returns>
        IProducerPlan Build();
    }
}