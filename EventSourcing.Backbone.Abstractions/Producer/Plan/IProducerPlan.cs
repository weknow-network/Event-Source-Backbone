using System.Collections.Immutable;

namespace EventSourcing.Backbone
{

    /// <summary>
    /// Plan contract
    /// </summary>
    /// <seealso cref="EventSourcing.Backbone.IProducerPlanBase" />
    public interface IProducerPlan : IProducerPlanBase
    {
        /// <summary>
        /// Gets the communication channel provider.
        /// </summary>
        IProducerChannelProvider Channel { get; }

        /// <summary>
        /// Gets the storage strategy.
        /// By design the stream should hold minimal information while the main payload 
        /// is segmented and can stored outside of the stream.
        /// This pattern will help us to split data for different reasons, for example GDPR PII (personally identifiable information).
        /// </summary>
        ImmutableArray<IProducerStorageStrategyWithFilter> StorageStrategies { get; }

        /// <summary>
        /// Gets the forwards pipelines.
        /// Result of merging multiple channels.
        /// </summary>
        IImmutableList<IProducerPlan> ForwardPlans { get; }
    }
}