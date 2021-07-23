using Microsoft.Extensions.Logging;

using Polly;

using System;
using System.Collections.Immutable;
using System.Threading;

namespace Weknow.EventSource.Backbone
{

    /// <summary>
    /// The actual concrete plan
    /// </summary>
    public interface IConsumerPlan: IConsumerPlanBase
    {        /// <summary>
             /// Gets a communication channel provider factory.
             /// </summary>
        IConsumerChannelProvider Channel { get; }
    }
}