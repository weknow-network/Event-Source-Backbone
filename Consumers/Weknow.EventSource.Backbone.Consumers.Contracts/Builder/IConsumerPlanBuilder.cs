using Microsoft.Extensions.Logging;

using Polly;

using System;
using System.Collections.Immutable;
using System.Threading;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Building phase of the plan
    /// </summary>
    public interface IConsumerPlanBuilder : IConsumerPlanBase
    {
        /// <summary>
        /// Gets a communication channel provider factory.
        /// </summary>
        Func<ILogger, IConsumerChannelProvider> ChannelFactory { get; }

        /// <summary>
        /// Builds the plan.
        /// </summary>
        /// <returns></returns>
        IConsumerPlan Build();
    }
}