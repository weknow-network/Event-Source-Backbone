using System.Collections.Concurrent;

using EventSourcing.Backbone.Enums;

using Microsoft.Extensions.Logging;

namespace EventSourcing.Backbone;


/// <summary>
/// Represent single consuming subscription
/// </summary>
internal sealed class EventSourceSubscriber : IConsumerLifetime, IConsumerBridge
{
    private readonly CancellationTokenSource _disposeCancellation;
    private readonly TaskCompletionSource<object?> _completion;
    private readonly ValueTask _subscriptionLifetime;
    private readonly ConsumerOptions _options;
    private readonly uint _maxMessages;
    private readonly static ConcurrentDictionary<object, object?> _keepAlive = // prevent collection by the GC
                                        new ConcurrentDictionary<object, object?>();

    // counter of consuming attempt (either successful or faulted) not includes Polly retry policy
    private long _consumeCounter;
    private readonly IConsumerPlan _plan;
    private readonly ISubscriptionBridge _bridge;

    #region Ctor

    /// <summary>
    /// Initializes a new subscription.
    /// </summary>
    /// <param name="plan">The builder's plan.</param>
    /// <param name="subscription">The subscription target.</param>

    public EventSourceSubscriber(
        IConsumerPlan plan,
        ISubscriptionBridge subscription)
    {
        _disposeCancellation = new CancellationTokenSource();
        _completion = new TaskCompletionSource<object?>();
        _plan = plan;
        _bridge = subscription;
        var channel = plan.Channel;
        if (plan.Options.KeepAlive)
        {
            _keepAlive.TryAdd(this, null);
            _disposeCancellation.Token.Register(() => _keepAlive.TryRemove(this, out _));
        }

        _options = _plan.Options;
        _maxMessages = _options.MaxMessages;

        _subscriptionLifetime = channel.SubscribeAsync(
                                            plan,
                                            ConsumingAsync,
                                            _disposeCancellation.Token);

        _subscriptionLifetime.AsTask().ContinueWith(_ => DisposeAsync());
    }

    #endregion // Ctor

    private ILogger Logger => _plan.Logger;

    #region GetParameterAsync

    /// <summary>
    /// Get parameter value from the announcement.
    /// </summary>
    /// <typeparam name="TParam">The type of the parameter.</typeparam>
    /// <param name="arg">The argument.</param>
    /// <param name="argumentName">Name of the argument.</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    ValueTask<TParam> IConsumerBridge.GetParameterAsync<TParam>(Announcement arg, string argumentName) =>
                                                            _plan.GetParameterAsync<TParam>(arg, argumentName);

    #endregion // GetParameterAsync

    #region Completion

    /// <summary>
    /// Represent the consuming completion..
    /// </summary>
    public Task Completion => _completion.Task;

    #endregion // Completion

    #region ConsumingAsync

    /// <summary>
    /// Handles consuming of single event.
    /// </summary>
    /// <param name="announcement">The argument.</param>
    /// <param name="ack">
    /// The acknowledge callback which will prevent message from 
    /// being re-fetch from same consumer group.</param>
    /// <returns></returns>
    private async ValueTask<bool> ConsumingAsync(
        Announcement announcement,
        IAck ack)
    {
        CancellationToken cancellation = _plan.Cancellation;
        Metadata meta = announcement.Metadata;

        #region Increment & Validation Max Messages Limit

        long count = Interlocked.Increment(ref _consumeCounter);
        if (_maxMessages != 0 && _maxMessages < count)
        {
            await DisposeAsync();
            throw new OperationCanceledException();
        }

        #endregion // Increment & Validation Max Messages Limit

        var consumerMeta = new ConsumerMetadata(meta, _plan.Options, cancellation);

        var logger = Logger;

        #region _plan.Interceptors.InterceptAsync(...)

        Bucket interceptionBucket = announcement.InterceptorsData;
        foreach (var interceptor in _plan.Interceptors)
        {
            if (!interceptionBucket.TryGetValue(
                                    interceptor.InterceptorName,
                                    out ReadOnlyMemory<byte> interceptedData))
            {
                interceptedData = ReadOnlyMemory<byte>.Empty;
            }
            await interceptor.InterceptAsync(meta, interceptedData);
        }

        #endregion // _plan.Interceptors.InterceptAsync(...)

        PartialConsumerBehavior partialBehavior = _options.PartialBehavior;

        bool hasProcessed = false;
        try
        {
            await using (Ack.Set(ack))
            {
                hasProcessed = await _plan.ResiliencePolicy.ExecuteAsync<bool>(async (ct) =>
                {
                    if (ct.IsCancellationRequested) return false;

                    ConsumerMetadata._metaContext.Value = consumerMeta;
                    if (await _bridge.BridgeAsync(announcement, this))
                        return true;
                    return false;
                }, cancellation);

                var options = _plan.Options;
                var behavior = options.AckBehavior;
                if (partialBehavior == PartialConsumerBehavior.ThrowIfNotHandled && !hasProcessed)
                {
                    Logger.LogCritical("No handler is matching event: {stream}, operation{operation}, MessageId:{id}", meta.FullUri(), meta.Operation, meta.MessageId);
                    throw new InvalidOperationException($"No handler is matching event: {meta.FullUri()}, operation{meta.Operation}, MessageId:{meta.MessageId}");
                }
                if (hasProcessed && behavior == AckBehavior.OnSucceed)
                {
                    await ack.AckAsync(AckBehavior.OnSucceed);
                }
            }
        }
        #region Exception Handling

        catch (OperationCanceledException)
        {
            logger.LogWarning("Canceled event: {0}", meta.FullUri());
            if (_plan.Options.AckBehavior != AckBehavior.OnFinally)
            {
                await ack.CancelAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "event: {0}", meta.FullUri());
            if (_plan.Options.AckBehavior != AckBehavior.OnFinally)
            {
                await ack.CancelAsync();
                throw;
            }
        }

        #endregion // Exception Handling
        finally
        {
            if (_plan.Options.AckBehavior == AckBehavior.OnFinally && partialBehavior != PartialConsumerBehavior.Sequential)
            {
                await ack.AckAsync(_plan.Options.AckBehavior);
            }

            #region Validation Max Messages Limit

            if (_maxMessages != 0 && _maxMessages <= count)
            {
                await DisposeAsync();
            }

            #endregion // Validation Max Messages Limit
        }

        return hasProcessed;
    }

    #endregion // ConsumingAsync

    #region DisposeAsync

    /// <summary>
    /// Release consumer.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous dispose operation.
    /// </returns>
    public ValueTask DisposeAsync()
    {
        #region Validation

        if (_disposeCancellation.IsCancellationRequested)
            return ValueTask.CompletedTask;

        #endregion // Validation

        _disposeCancellation.CancelSafe();
        _completion.SetResult(null);
        return _subscriptionLifetime;
    }

    #endregion // DisposeAsync
}
