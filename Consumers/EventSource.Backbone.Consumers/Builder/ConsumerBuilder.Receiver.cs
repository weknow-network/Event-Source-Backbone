using System.Diagnostics;
using System.Text.Json;

using EventSource.Backbone.Building;

namespace EventSource.Backbone
{
    public partial class ConsumerBuilder
    {
        #region private class Receiver

        /// <summary>
        /// Receive data (on demand data query).
        /// </summary>
        [DebuggerDisplay("{_plan.Environment}:{_plan.Partition}:{_plan.Shard}")]
        private class Receiver : IConsumerReceiver
        {
            private readonly IConsumerPlan _plan;

            #region Ctor

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="plan">The plan.</param>
            public Receiver(IConsumerPlan plan)
            {
                _plan = plan;
            }

            #endregion // Ctor

            #region Environment

            /// <summary>
            /// Include the environment as prefix of the stream key.
            /// for example: production:partition-name:shard-name
            /// </summary>
            /// <param name="environment">The environment (null: keep current environment, empty: reset the environment to nothing).</param>
            /// <returns></returns>
            IConsumerReceiver IConsumerEnvironmentOfBuilder<IConsumerReceiver>.Environment(Env? environment)
            {
                if (environment == null)
                    return this;

                IConsumerPlan plan = _plan.ChangeEnvironment(environment);
                var result = new Receiver(plan);
                return result;
            }

            #endregion // Environment


            /// <summary>
            /// replace the partition of the stream key.
            /// for example: production:partition-name:shard-name
            /// </summary>
            /// <param name="partition">The partition.</param>
            /// <returns></returns>
            IConsumerReceiver IConsumerPartitionBuilder<IConsumerReceiver>.Uri(string partition)
            {
                IConsumerPlan plan = _plan.ChangeKey(partition);
                var result = new Receiver(plan);
                return result;
            }

            #region GetByIdAsync

            /// <summary>
            /// Gets the asynchronous.
            /// </summary>
            /// <param name="entryId">The entry identifier.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns></returns>
            async ValueTask<AnnouncementData> IConsumerReceiverCommands.GetByIdAsync(
                            EventKey entryId,
                            CancellationToken cancellationToken)
            {
                var channel = _plan.Channel;
                AnnouncementData result = await channel.GetByIdAsync(entryId, _plan, cancellationToken);
                return result;
            }

            #endregion // GetByIdAsync

            #region GetJsonByIdAsync

            /// <summary>
            /// Gets the asynchronous.
            /// </summary>
            /// <param name="entryId">The entry identifier.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns></returns>
            async ValueTask<JsonElement> IConsumerReceiverCommands.GetJsonByIdAsync(
                            EventKey entryId,
                            CancellationToken cancellationToken)
            {
                var channel = _plan.Channel;
                AnnouncementData announcement = await channel.GetByIdAsync(entryId, _plan, cancellationToken);

                JsonElement result = ToJson(_plan, announcement);
                return result;
            }

            #endregion // GetJsonByIdAsync
        }

        #endregion // private class Receiver
    }
}
