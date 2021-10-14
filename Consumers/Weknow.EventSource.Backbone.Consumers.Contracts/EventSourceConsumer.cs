
using System;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Base class for the consumer's code generator
    /// </summary>
    public static class EventSourceConsumer
    {
        #region GetParameterAsync

        /// <summary>
        /// Get parameter value from the announcement.
        /// </summary>
        /// <typeparam name="TParam">The type of the parameter.</typeparam>
        /// <param name="arg">The argument.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="plan">The plan.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static async ValueTask<TParam> GetParameterAsync<TParam>(Announcement arg, string argumentName, IConsumerPlan plan)
        {
            foreach (var strategy in plan.SegmentationStrategies)
            {
                var (isValid, value) = await strategy.TryUnclassifyAsync<TParam>(arg.Segments, arg.Metadata.Operation, argumentName, plan.Options);
                if (isValid)
                    return value;
            }
            throw new NotSupportedException();
        }

        #endregion // GetParameterAsync
    }
}
