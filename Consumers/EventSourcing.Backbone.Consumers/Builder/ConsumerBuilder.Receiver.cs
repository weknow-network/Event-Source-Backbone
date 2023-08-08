using System.Diagnostics;
using System.Text.Json;

using EventSourcing.Backbone.Building;

namespace EventSourcing.Backbone
{
    public partial class ConsumerBuilder
    {
        /// <summary>
        /// Receive data (on demand data query).
        /// </summary>
        [DebuggerDisplay("{_plan.Environment}:{_plan.Uri}")]
        private sealed class Receiver : IConsumerReceiver
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
            /// Environments from variable.
            /// </summary>
            /// <param name="environmentVariableKey">The environment variable key.</param>
            /// <returns></returns>
            /// <exception cref="EventSourcing.Backbone.EventSourcingException">EnvironmentFromVariable failed, [{environmentVariableKey}] not found!</exception>
            IConsumerReceiver IConsumerEnvironmentOfBuilder<IConsumerReceiver>.EnvironmentFromVariable(string environmentVariableKey)
            {
                string environment = Environment.GetEnvironmentVariable(environmentVariableKey) ?? throw new EventSourcingException($"EnvironmentFromVariable failed, [{environmentVariableKey}] not found!");


                IConsumerPlan plan = _plan.ChangeEnvironment(environment);
                var result = new Receiver(plan);
                return result;
            }

            #endregion // Environment

            #region Environment

            /// <summary>
            /// Include the environment as prefix of the stream key.
            /// for example: env:URI
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

            #region Uri

            /// <summary>
            /// replace the URI of the stream key.
            /// for example: env:URI
            /// </summary>
            /// <param name="uri">The URI.</param>
            /// <returns></returns>
            IConsumerReceiver IConsumerUriBuilder<IConsumerReceiver>.Uri(string uri)
            {
                IConsumerPlan plan = _plan.ChangeKey(uri);
                var result = new Receiver(plan);
                return result;
            }

            #endregion // Uri

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
    }
}
