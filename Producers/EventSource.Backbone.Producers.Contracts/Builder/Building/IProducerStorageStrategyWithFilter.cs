namespace EventSource.Backbone
{
    /// <summary>
    /// Responsible to save information to storage.
    /// The information can be either Segmentation or Interception.
    /// When adding it via the builder it can be arrange in a chain in order of having
    /// 'Chain of Responsibility' for saving different parts into different storage (For example GDPR's PII).
    /// Alternative, chain can serve as a cache layer.
    /// </summary>
    public interface IProducerStorageStrategyWithFilter : IProducerStorageStrategy
    {
        /// <summary>
        /// Determines whether is of the right target type.
        /// </summary>
        /// <param name="type">The type.</param>

        bool IsOfTargetType(EventBucketCategories type);
    }
}
