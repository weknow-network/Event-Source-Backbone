using System.Collections.Immutable;

namespace Weknow.EventSource.Backbone
{

    /// <summary>
    /// Plan contract
    /// </summary>
    /// <seealso cref="Weknow.EventSource.Backbone.IProducerPlanBase" />
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
        Task<ImmutableArray<IProducerStorageStrategyWithFilter>> StorageStrategiesAsync { get; }

        /// <summary>
        /// Gets the forwards pipelines.
        /// Result of merging multiple channels.
        /// </summary>
        IImmutableList<IProducerPlan> ForwardPlans { get; }
    }
}