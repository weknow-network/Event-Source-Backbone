using System.Text;
using System.Text.Json;

using EventSourcing.Backbone.Channels;

using Microsoft.Extensions.Logging;

using static EventSourcing.Backbone.EventSourceConstants;


namespace EventSourcing.Backbone;

/// <summary>
/// Responsible to load information from S3 storage.
/// The information can be either Segmentation or Interception.
/// When adding it via the builder it can be arrange in a chain in order of having
/// 'Chain of Responsibility' for saving different parts into different storage (For example GDPR's PII).
/// Alternative, chain can serve as a cache layer.
/// </summary>
public class S3ConsumerStorageStrategy : IConsumerStorageStrategy
{
    private readonly IS3Repository _repository;
    private readonly S3ConsumerStorageTuning _tune;

    #region ctor

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="s3Repository">S3 repository.
    /// Use S3Factory in order to create it (will create one if missing).
    /// S3Factory will read credentials from the following environment variables: "S3_EVENT_SOURCE_ACCESS_KEY", "S3_EVENT_SOURCE_SECRET", "S3_EVENT_SOURCE_REGION".</param>
    /// <param name="tune">Tuning options.</param>
    public S3ConsumerStorageStrategy(
        IS3Repository s3Repository,
        S3ConsumerStorageTuning tune)
    {
        _repository = s3Repository;
        _tune = tune;
    }

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The options.</param>
    /// <param name="factory">The repository's factory.</param>
    public S3ConsumerStorageStrategy(
        ILogger logger,
        S3ConsumerOptions? options = null,
        IS3RepositoryFactory? factory = null)
    {
        IS3RepositoryFactory facroey = factory ?? S3RepositoryFactory.Create(logger);
        var opt = options ?? S3ConsumerOptions.Default;
        _repository = facroey.Get(opt);
        _tune = new S3ConsumerStorageTuning { KeysFilter = opt.KeysFilter, OverrideKeyIfExists = opt.OverrideKeyIfExists };
    }

    #endregion // ctor

    /// <summary>
    /// Gets the name of the storage provider.
    /// </summary>
    public string Name { get; } = "S3";

    /// <summary>
    /// Load the bucket information.
    /// </summary>
    /// <param name="meta">The meta fetch provider.</param>
    /// <param name="prevBucket">The current bucket (previous item in the chain).</param>
    /// <param name="type">The type of the storage.</param>
    /// <param name="getProperty">The get property.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns>
    /// Either Segments or Interceptions.
    /// </returns>
    async ValueTask<Bucket> IConsumerStorageStrategy.LoadBucketAsync(
        Metadata meta,
        Bucket prevBucket,
        EventBucketCategories type,
        Func<string, string> getProperty,
        CancellationToken cancellation)
    {
        var filter = _tune.KeysFilter;
        var overrideExisting = _tune.OverrideKeyIfExists;

        string json = getProperty($"{Constants.PROVIDER_ID}~{type}");
        var keyPathPairs = JsonSerializer.Deserialize<KeyValuePair<string, string>[]>(
                                                        json, SerializerOptionsWithIndent) ??
                                                        Array.Empty<KeyValuePair<string, string>>();


        var tasks = keyPathPairs.Select(LocalFetchAsync);
        var pairs = await Task.WhenAll(tasks);

        Bucket result = prevBucket.TryAddRange(pairs);
        return result;

        async Task<(string key, byte[] value)?> LocalFetchAsync(KeyValuePair<string, string> item)
        {
            var (key, value ) = item;
            // honor filtering
            if (filter != null && !filter(key))
                return null;

            // avoid overriding existing keys
            if(!overrideExisting && prevBucket.ContainsKey(key))
                return null;

            string path = value;
            var response = await _repository.GetBytesAsync(meta.Environment, path, cancellation);
            return (key, response);
        }
    }
}
