using System.Collections.Immutable;

using EventSource.Backbone.Building;

namespace EventSource.Backbone
{

    /// <summary>
    /// The actual concrete plan
    /// </summary>
    public interface IConsumerPlan : IConsumerPlanBase
    {        /// <summary>
             /// Gets a communication channel provider factory.
             /// </summary>
        IConsumerChannelProvider Channel { get; }

        /// <summary>
        /// change the environment.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <returns>An IConsumerPlan.</returns>
        IConsumerPlan ChangeEnvironment(Env? environment);

        /// <summary>
        /// change the stream's key (identity).
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <returns>An IConsumerPlan.</returns>
        IConsumerPlan ChangeKey(string? partition);

        /// <summary>
        /// Gets the storage strategies.
        /// </summary>
        Task<ImmutableArray<IConsumerStorageStrategyWithFilter>> StorageStrategiesAsync { get; }

        /// <summary>
        /// Get parameter value from the announcement.
        /// </summary>
        /// <typeparam name="TParam">The type of the parameter.</typeparam>
        /// <param name="arg">The argument.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        ValueTask<TParam> GetParameterAsync<TParam>(Announcement arg, string argumentName);
    }
}