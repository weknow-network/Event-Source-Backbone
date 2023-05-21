using EventSourcing.Backbone.Building;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Receive data (on demand data query).
    /// </summary>
    public interface IConsumerIterator :
        IConsumerEnvironmentOfBuilder<IConsumerIterator>,
        IConsumerPartitionBuilder<IConsumerIterator>,
        IConsumerIteratorCommands
    {
        /// <summary>
        /// Get specialized iterator.
        /// </summary>
        /// <typeparam name="TEntityFamily">This type is used for filtering the result, only result of this type will yield.</typeparam>
        /// <param name="mapper">The mapper.</param>
        /// <returns></returns>
        IConsumerIterator<TEntityFamily> Specialize<TEntityFamily>(IConsumerEntityMapper<TEntityFamily> mapper);
    }

    /// <summary>
    /// Receive data (on demand data query).
    /// </summary>
    public interface IConsumerIterator<TEntityFamily> :
        IConsumerIteratorCommands,
        IConsumerIteratorCommands<TEntityFamily>
    {
    }
}
