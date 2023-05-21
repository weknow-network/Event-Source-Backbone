namespace EventSource.Backbone
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