using System.Collections.Immutable;

using EventSourcing.Backbone.Building;

using Microsoft.Extensions.Logging;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Plan builder contract
    /// </summary>
    /// <seealso cref="EventSourcing.Backbone.IProducerPlanBase" />
    public interface IProducerPlanBuilder : IProducerPlanBase
    {
        /// <summary>
        /// Gets the communication channel provider factory.
        /// </summary>
        Func<ILogger, IProducerChannelProvider> ChannelFactory { get; }

        /// <summary>
        /// Gets the storage strategy.
        /// By design the stream should hold minimal information while the main payload 
        /// is segmented and can stored outside of the stream.
        /// This pattern will help us to split data for different reasons, for example GDPR PII (personally identifiable information).
        /// </summary>
        ImmutableArray<Func<ILogger, IProducerStorageStrategy>> StorageStrategyFactories { get; }

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