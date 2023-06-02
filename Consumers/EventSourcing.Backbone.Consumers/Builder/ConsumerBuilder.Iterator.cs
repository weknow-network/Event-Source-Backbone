using System.Diagnostics;
using System.Runtime.CompilerServices;
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
        private sealed class Iterator : IConsumerIterator
        {
            private readonly IConsumerPlan _plan;

            #region Ctor

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="plan">The plan.</param>
            public Iterator(IConsumerPlan plan)
            {
                _plan = plan;
            }

            #endregion // Ctor

            #region Environment

            /// <summary>
            /// Include the environment as prefix of the stream key.
            /// for example: env:URI
            /// </summary>
            /// <param name="environment">The environment (null: keep current environment, empty: reset the environment to nothing).</param>
            /// <returns></returns>
            IConsumerIterator IConsumerEnvironmentOfBuilder<IConsumerIterator>.Environment(Env? environment)
            {
                if (environment == null)
                    return this;

                IConsumerPlan plan = _plan.ChangeEnvironment(environment);
                var result = new Iterator(plan);
                return result;
            }

            #endregion // Environment

            #region Environment

            /// <summary>
            /// Fetch the origin environment of the message from an environment variable.
            /// </summary>
            /// <param name="environmentVariableKey">The environment variable key.</param>
            /// <returns></returns>
            IConsumerIterator IConsumerEnvironmentOfBuilder<IConsumerIterator>.EnvironmentFromVariable(string environmentVariableKey)
            {
                string environment = Environment.GetEnvironmentVariable(environmentVariableKey) ?? throw new EventSourcingException($"EnvironmentFromVariable failed, [{environmentVariableKey}] not found!");

                IConsumerPlan plan = _plan.ChangeEnvironment(environment);
                var result = new Iterator(plan);
                return result;
            }

            #endregion // Environment

            #region Uri

            /// <summary>
            /// replace the stream's key (identity).
            /// for example: env:URI
            /// </summary>
            /// <param name="uri">The URI.</param>
            /// <returns></returns>
            IConsumerIterator IConsumerUriBuilder<IConsumerIterator>.Uri(string uri)
            {
                IConsumerPlan plan = _plan.ChangeKey(uri);
                var result = new Iterator(plan);
                return result;
            }

            #endregion // Uri

            #region GetAsyncEnumerable

            /// <summary>
            /// Gets asynchronous enumerable of announcements.
            /// </summary>
            /// <param name="options">The options.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns></returns>
            async IAsyncEnumerable<Announcement> IConsumerIteratorCommands.GetAsyncEnumerable(
                ConsumerAsyncEnumerableOptions? options,
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                var loop = _plan.Channel.GetAsyncEnumerable(_plan, options, cancellationToken).WithCancellation(cancellationToken);
                await foreach (var announcement in loop.WithCancellation(cancellationToken))
                {
                    if (cancellationToken.IsCancellationRequested) yield break;

                    yield return announcement;
                }
            }

            #endregion // GetAsyncEnumerable

            #region GetJsonAsyncEnumerable

            /// <summary>
            /// Gets asynchronous enumerable of announcements.
            /// </summary>
            /// <param name="options">The options.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns></returns>
            async IAsyncEnumerable<JsonElement> IConsumerIteratorCommands.GetJsonAsyncEnumerable(
                ConsumerAsyncEnumerableJsonOptions? options,
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                var loop = _plan.Channel.GetAsyncEnumerable(_plan, options, cancellationToken);
                await foreach (Announcement announcement in loop.WithCancellation(cancellationToken))
                {
                    JsonElement result = ToJson(_plan, announcement, options?.IgnoreMetadata ?? false);
                    yield return result;
                }
            }

            #endregion // GetJsonAsyncEnumerable

            #region Specialize

            /// <summary>
            /// Get specialized iterator.
            /// </summary>
            /// <typeparam name="TEntityFamily">This type is used for filtering the result, only result of this type will yield.</typeparam>
            /// <param name="mapper">The mapper.</param>
            /// <returns></returns>
            IConsumerIterator<TEntityFamily> IConsumerIterator.Specialize<TEntityFamily>(IConsumerEntityMapper<TEntityFamily> mapper)
            {
                return new Iterator<TEntityFamily>(_plan, this, mapper);
            }

            #endregion // Specialize
        }

        /// <summary>
        /// Receive data (on demand data query).
        /// </summary>
        [DebuggerDisplay("{_plan.Environment}:{_plan.Uri}")]
        private sealed class Iterator<TEntityFamily> : IConsumerIterator<TEntityFamily>
        {
            private readonly IConsumerPlan _plan;
            private readonly IConsumerIterator _iterator;
            private readonly IConsumerEntityMapper<TEntityFamily> _mapper;

            #region Ctor

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="plan">The plan.</param>
            /// <param name="iterator">The iterator.</param>
            /// <param name="mapper">The mapper.</param>
            public Iterator(
                IConsumerPlan plan,
                IConsumerIterator iterator,
                IConsumerEntityMapper<TEntityFamily> mapper)
            {
                _plan = plan;
                _iterator = iterator;
                _mapper = mapper;
            }

            #endregion // Ctor

            #region GetAsyncEnumerable

            /// <summary>
            /// Gets asynchronous enumerable of announcements.
            /// </summary>
            /// <param name="options">The options.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns></returns>
            IAsyncEnumerable<Announcement> IConsumerIteratorCommands.GetAsyncEnumerable(
                ConsumerAsyncEnumerableOptions? options,
                CancellationToken cancellationToken)
            {
                return _iterator.GetAsyncEnumerable(options, cancellationToken);
            }

            #endregion // GetAsyncEnumerable

            #region GetJsonAsyncEnumerable

            /// <summary>
            /// Gets asynchronous enumerable of announcements.
            /// </summary>
            /// <param name="options">The options.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns></returns>
            IAsyncEnumerable<JsonElement> IConsumerIteratorCommands.GetJsonAsyncEnumerable(
                ConsumerAsyncEnumerableJsonOptions? options,
                CancellationToken cancellationToken)
            {
                return _iterator.GetJsonAsyncEnumerable(options, cancellationToken);
            }

            #endregion // GetJsonAsyncEnumerable

            #region GetAsyncEnumerable<TCast>

            /// <summary>
            /// Gets asynchronous enumerable of announcements.
            /// </summary>
            /// <typeparam name="TCast">This type is used for filtering the result, only result of this type will yield.</typeparam>
            /// <param name="options">The options.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns></returns>
            async IAsyncEnumerable<TCast> IConsumerIteratorCommands<TEntityFamily>.GetAsyncEnumerable<TCast>(
                ConsumerAsyncEnumerableOptions? options,
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                var loop = _plan.Channel.GetAsyncEnumerable(_plan, options, cancellationToken);
                await foreach (Announcement announcement in loop.WithCancellation(cancellationToken))
                {
                    Predicate<Metadata>? filter = options?.OperationFilter;
                    if (filter != null && !filter(announcement.Metadata))
                        continue;

                    var (result, succeed) = await _mapper.TryMapAsync<TCast>(announcement, _plan);

                    if (succeed && result != null)
                        yield return result;
                }
            }

            #endregion // GetAsyncEnumerable<TCast>
        }
    }
}
