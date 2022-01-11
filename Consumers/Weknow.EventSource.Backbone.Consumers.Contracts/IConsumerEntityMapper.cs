using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Responsible of translating announcement's parameter into object
    /// </summary>
    /// <typeparam name="T">Represent the DTO's family</typeparam>
    public interface IConsumerEntityMapper<T>
    {
        /// <summary>
        /// Maps the asynchronous.
        /// </summary>
        /// <param name="announcement">The announcement.</param>
        /// <param name="consumerPlan">The consumer plan.</param>
        /// <returns></returns>
        Task<T> MapAsync(Announcement announcement, IConsumerPlan consumerPlan);

        /// <summary>
        /// Try to map announcement.
        /// </summary>
        /// <typeparam name="TCast">Cast target</typeparam>
        /// <param name="announcement">The announcement.</param>
        /// <param name="consumerPlan">The consumer plan.</param>
        /// <param name="operation">Optional operation filter (useful when method have same type of parameters).</param>
        /// <returns></returns>
        Task<(TCast? value, bool succeed)> TryMapAsync<TCast>(Announcement announcement, IConsumerPlan consumerPlan, Predicate<string>? operation = null) where TCast: T;
    }
}
