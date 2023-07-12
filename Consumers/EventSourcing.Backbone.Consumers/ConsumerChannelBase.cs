using Microsoft.Extensions.Logging;

namespace EventSourcing.Backbone.Private;

/// <summary>
/// The redis consumer channel.
/// </summary>
public abstract class ConsumerChannelBase // : IConsumerChannelProvider
{
    protected readonly ILogger _logger;

    #region Ctor

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="logger">The logger.</param>
    protected ConsumerChannelBase(ILogger logger)
    {
        _logger = logger;
    }

    #endregion // Ctor

    #region SubsribeAsync

    /// <summary>
    /// Subscribe to the channel for specific metadata.
    /// </summary>
    /// <param name="plan">The consumer plan.</param>
    /// <param name="func">The function.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// When completed
    /// </returns>
    public async ValueTask SubscribeAsync(
                IConsumerPlan plan,
                Func<Announcement, IAck, ValueTask<bool>> func,
                CancellationToken cancellationToken)
    {
        var joinCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(plan.Cancellation, cancellationToken);
        var joinCancellation = joinCancellationSource.Token;
        ConsumerOptions options = plan.Options;

        ILogger? logger = _logger ?? plan.Logger;
        logger.LogInformation("REDIS EVENT-SOURCE | SUBSCRIBE key: [{key}], consumer-group: [{consumer-group}], consumer-name: [{consumer-name}]", plan.FullUri(), plan.ConsumerGroup, plan.ConsumerName);

        while (!joinCancellation.IsCancellationRequested)
        {
            try
            {
                await OnSubscribeToSingleAsync(plan, func, options, joinCancellation);
                // TODO: [bnaya 2023-05-22] think of the api for multi stream subscription (by partial uri * pattern) ->  var keys = GetKeysUnsafeAsync(pattern: $"{partition}:*").WithCancellation(cancellationToken)

                if (options.FetchUntilUnixDateOrEmpty != null)
                    break;
            }
            #region Exception Handling

            catch (OperationCanceledException)
            {
                if (_logger == null)
                    Console.WriteLine($"Subscribe cancellation [{plan.FullUri()}] event stream (may have reach the messages limit)");
                else
                    _logger.LogError("Subscribe cancellation [{uri}] event stream (may have reach the messages limit)",
                        plan.Uri);
                joinCancellationSource.CancelSafe();
            }
            catch (Exception ex)
            {
                if (_logger == null)
                    Console.WriteLine($"Fail to subscribe into the [{plan.FullUri()}] event stream");
                else
                    _logger.LogError(ex, "Fail to subscribe into the [{uri}] event stream",
                        plan.Uri);
                throw;
            }

            #endregion // Exception Handling
        }
    }

    #endregion // SubsribeAsync

    #region OnSubscribeToSingleAsync

    /// <summary>
    /// Subscribe to specific shard.
    /// </summary>
    /// <param name="plan">The consumer plan.</param>
    /// <param name="func">The function.</param>
    /// <param name="options">The options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    protected abstract Task OnSubscribeToSingleAsync(
                IConsumerPlan plan,
                Func<Announcement, IAck, ValueTask<bool>> func,
                ConsumerOptions options,
                CancellationToken cancellationToken);


    #endregion // OnSubscribeToSingleAsync
}
