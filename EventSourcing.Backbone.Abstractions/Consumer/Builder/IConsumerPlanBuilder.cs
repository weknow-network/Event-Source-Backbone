﻿using System.Collections.Immutable;

using EventSourcing.Backbone.Building;

using Microsoft.Extensions.Logging;

namespace EventSourcing.Backbone
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
        /// Gets the storage strategy factories.
        /// </summary>
        ImmutableArray<Func<ILogger, Task<IConsumerStorageStrategyWithFilter>>> StorageStrategyFactories { get; }

        /// <summary>
        /// Builds the plan.
        /// </summary>
        /// <returns></returns>
        IConsumerPlan Build();
    }
}