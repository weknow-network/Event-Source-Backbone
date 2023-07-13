using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Metrics;

using Microsoft.Extensions.Logging;

using Polly;

using static EventSourcing.Backbone.Private.EventSourceTelemetry;

namespace EventSourcing.Backbone.Producers;

public abstract class ProducerChannelBase : IProducerChannelProvider
{
    protected readonly ILogger _logger;
    protected readonly AsyncPolicy _resiliencePolicy;

    private static readonly Counter<int> ProduceEventsCounter = EMeter.CreateCounter<int>("evt-src.sys.produce.events", "count",
                                            "Sum of total produced events (messages)");
    private static readonly Counter<int> ProduceEventsErrorCounter = EMeter.CreateCounter<int>("evt-src.sys.produce.events.error", "count",
                                            "Sum of total errors on produced events (messages)");

    #region Ctor

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="resiliencePolicy">The resilience policy for retry.</param>
    protected ProducerChannelBase(
                    ILogger logger,
                    AsyncPolicy? resiliencePolicy)
    {
        _logger = logger;
        _resiliencePolicy = resiliencePolicy ??
                            Policy.Handle<Exception>()
                                  .RetryAsync(3);
    }


    #endregion // Ctor

    #region DefaultStorageStrategy

    /// <summary>
    /// Gets the default storage strategy.
    /// </summary>
    protected virtual IProducerStorageStrategy DefaultStorageStrategy => throw new EventSourcingException("Storage is missing (you should define a storage in the builder, no default storage for this channel)");

    #endregion // DefaultStorageStrategy

    #region ChannelType

    /// <summary>
    /// Gets the type of the channel.
    /// </summary>
    protected abstract string ChannelType { get; }

    #endregion // ChannelType

    #region OnSendAsync

    /// <summary>
    /// Called when [send asynchronous].
    /// </summary>
    /// <param name="plan">The plan.</param>
    /// <param name="payload">The raw announcement data.</param>
    /// <param name="storageMeta">
    /// The storage meta information, like the bucket name in case of s3, etc.
    /// </param>
    /// <returns></returns>
    protected abstract ValueTask<string> OnSendAsync(
        IProducerPlan plan,
        Announcement payload,
        IImmutableDictionary<string, string> storageMeta);

    #endregion // OnSendAsync

    #region SendAsync

    /// <summary>
    /// Sends raw announcement.
    /// </summary>
    /// <param name="plan">The plan.</param>
    /// <param name="payload">The raw announcement data.</param>
    /// <returns>
    /// Return the message id
    /// </returns>
    async ValueTask<string> IProducerChannelProvider.SendAsync(
        IProducerPlan plan,
        Announcement payload)
    {
        Metadata meta = payload.Metadata;
        // TODO: [bnaya 2023-07-10] Make it relevant within `OnSendAsync`: meta = meta with { ChannelType = ChannelType };

        string id = meta.MessageId;
        string env = meta.Environment.ToDash();
        string uri = meta.UriDash;

        try
        {
            using var activity = plan.StartTraceInformation($"producer.{meta.Operation}.process",
                                                tagsAction: t => t.Add("env", env)
                                                                .Add("uri", uri)
                                                                .Add("message-id", id));
            ProduceEventsCounter.WithTag("uri", uri).WithTag("env", env).Add(1);

            string? messageId = await _resiliencePolicy.ExecuteAsync(LocalAsync);
            return messageId ?? "0000000000000-0";

        }
        #region Exception Handling

        catch (Exception ex)
        {
            ProduceEventsErrorCounter.WithTag("uri", uri).WithTag("env", env).Add(1);
            _logger.LogError(ex, "Fail to produce event: env={env}, uri={uri}, oprtation={operation}", env, meta.Uri, meta.Operation);
            throw;
        }

        #endregion // Exception Handling

        async Task<string> LocalAsync()
        {
            // The storage meta information, like the bucket name in case of s3, etc.
            ImmutableDictionary<string, string>.Builder storageMeta = await SaveToStorageAsync(plan, payload);
            storageMeta.Add(nameof(meta.MessageId), id);
            storageMeta.Add(nameof(meta.Operation), meta.Operation);
            storageMeta.Add(nameof(meta.ChannelType), ChannelType);
#pragma warning disable HAA0102 
            storageMeta.Add(nameof(meta.Origin), meta.Origin.ToString());
#pragma warning restore HAA0102 

            using Activity? activity = plan.StartProducerTrace(meta);

            var result = await OnSendAsync(plan, payload, storageMeta.ToImmutable());
            return result;
        }
    }

    #endregion // SendAsync

    #region SaveToStorageAsync

    /// <summary>
    /// Sends raw announcement.
    /// </summary>
    /// <param name="plan">The plan.</param>
    /// <param name="payload">The raw announcement data.</param>
    /// <returns>
    /// The storage meta information, like the bucket name in case of s3, etc.
    /// </returns>
    private async ValueTask<ImmutableDictionary<string, string>.Builder> SaveToStorageAsync(
        IProducerPlan plan,
        Announcement payload)
    {
        Metadata meta = payload.Metadata;
        string id = meta.MessageId;
        ImmutableArray<IProducerStorageStrategy> storageStrategy = plan.StorageStrategies;

        var results = ImmutableDictionary.CreateBuilder<string, string>();
        var storageMeta = await Task.WhenAll(LocalStoreBucketAsync(EventBucketCategories.Segments),
                            LocalStoreBucketAsync(EventBucketCategories.Interceptions))
                    .ThrowAll();
        foreach (var m in storageMeta)
        {
            results.AddRange(m);
        }
        return results;

        async Task<IImmutableDictionary<string, string>> LocalStoreBucketAsync(EventBucketCategories storageType)
        {
            var strategies = storageStrategy.Where(m => m.IsOfCategory(storageType));
            Bucket bucket = storageType == EventBucketCategories.Segments ? payload.Segments : payload.InterceptorsData;

            if (bucket.IsEmpty)
                return ImmutableDictionary<string, string>.Empty;

            if (strategies.Any())
            {
                var tasks = strategies.Select(strategy => SaveBucketAsync(strategy));
                var storageMetaInfoCollection = await Task.WhenAll(tasks).ThrowAll();
                ImmutableDictionary<string, string>.Builder metaBuilder = ImmutableDictionary.CreateBuilder<string, string>();
                foreach (var m in storageMetaInfoCollection)
                {
                    metaBuilder.AddRange(m);
                }
                var result = metaBuilder.ToImmutable();
                return result;
            }
            else
            {
                var result = await SaveBucketAsync(DefaultStorageStrategy);
                return result;
            }

            async Task<IImmutableDictionary<string, string>> SaveBucketAsync(IProducerStorageStrategy storage)
            {
                using (plan.StartTraceDebug($"producer.{storage.Name}-storage.{storageType}.set"))
                {
                    IImmutableDictionary<string, string> metaItems =
                        await storage.SaveBucketAsync(id, bucket, storageType, meta);
                    return metaItems;
                }
            }
        }
    }

    #endregion // SaveToStorageAsync
}
