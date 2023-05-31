using System.Collections.Immutable;
using System.Net;
using System.Text.Json;

using Amazon.S3;
using Amazon.S3.Model;

using Microsoft.Extensions.Logging;

using static EventSourcing.Backbone.EventSourceConstants;

namespace EventSourcing.Backbone.Channels
{

    /// <summary>
    /// Abstract S3 operations
    /// </summary>
    public sealed class S3Repository : IS3Repository, IDisposable
    {
        private static readonly string BUCKET =
            Environment.GetEnvironmentVariable("S3_EVENT_SOURCE_BUCKET")
            ?? string.Empty;

        private readonly string _bucket;
        private readonly ILogger _logger;
        private readonly string? _basePath;
        private readonly IAmazonS3 _client;
        private static readonly List<Tag> EMPTY_TAGS = new List<Tag>();
        private int _disposeCount = 0;
        private readonly S3EnvironmentConvention _environmentConvension;
        private const StringComparison STRING_COMPARISON = StringComparison.OrdinalIgnoreCase;
        private readonly bool _dryRun;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="client">S3 client.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The s3 options.</param>
        public S3Repository(
                    IAmazonS3 client,
                    ILogger logger,
                    S3Options options = default)
        {
            _client = client;
            _logger = logger;
            _bucket = options.Bucket ?? BUCKET;
            _basePath = options.BasePath;
            _environmentConvension = options.EnvironmentConvension;
            _dryRun = options.DryRun;
        }

        #endregion // Ctor

        #region AddReference

        /// <summary>
        /// Adds the reference to the repository.
        /// This reference will prevent disposal until having no active references.
        /// </summary>
        internal void AddReference() => Interlocked.Increment(ref _disposeCount);

        #endregion // AddReference

        #region GetJsonAsync

        /// <summary>
        /// Get content.
        /// </summary>
        /// <param name="env">Environment</param>
        /// <param name="id">The identifier which is the S3 key.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.InvalidDataException"></exception>
        public async ValueTask<JsonElement> GetJsonAsync(Env env, string id, CancellationToken cancellation = default)
        {
            try
            {
                using Stream srm = await GetStreamAsync(env, id, cancellation);

                var response = await JsonDocument.ParseAsync(srm);
                return response.RootElement;
            }
            #region Exception Handling

            catch (Exception e)
            {
                string msg = $"S3 Failed to parse json, env:{env}, id:{id}";
                _logger.LogError(e.FormatLazy(), msg);
                throw new InvalidDataException(msg);
            }

            #endregion // Exception Handling
        }

        #endregion // GetJsonAsync

        #region GetBytesAsync

        /// <summary>
        /// Get content.
        /// </summary>
        /// <param name="env">Environment</param>
        /// <param name="id">The identifier which is the S3 key.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.InvalidDataException"></exception>
        public async ValueTask<byte[]> GetBytesAsync(Env env, string id, CancellationToken cancellation = default)
        {
            try
            {
                using Stream srm = await GetStreamAsync(env, id, cancellation);
                using MemoryStream memStream = new();
                await srm.CopyToAsync(memStream);
                return memStream.ToArray();
            }
            #region Exception Handling

            catch (Exception e)
            {
                string msg = $"S3 Failed to parse bytes, env:{env}, id:{id}";
                _logger.LogError(e.FormatLazy(), msg);
                throw new InvalidDataException(msg);
            }

            #endregion // Exception Handling
        }

        #endregion // GetBytesAsync

        #region GetAsync

        /// <summary>
        /// Get content.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="env">Environment</param>
        /// <param name="id">The identifier which is the S3 key.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        /// <exception cref="System.NullReferenceException">Failed to deserialize industries</exception>
        /// <exception cref="System.IO.InvalidDataException"></exception>
        public async ValueTask<T> GetAsync<T>(Env env, string id, CancellationToken cancellation = default)
        {
            try
            {
                using Stream srm = await GetStreamAsync(env, id, cancellation);

                var response = await JsonSerializer.DeserializeAsync<T>(srm, SerializerOptionsWithIndent);

                #region Validation

                if (response == null)
                {
                    throw new EventSourcingException("Failed to deserialize industries");
                }

                #endregion // Validation
                return response;
            }
            #region Exception Handling

            catch (Exception e)
            {
                string msg = $"S3 Failed to deserialize into [{typeof(T).Name}], env:{env}, id:{id}";
                _logger.LogError(e.FormatLazy(), msg);
                throw new InvalidDataException(msg);
            }

            #endregion // Exception Handling
        }

        #endregion // GetAsync

        #region GetStreamAsync

        /// <summary>
        /// Get content.
        /// </summary>
        /// <param name="env">environment</param>
        /// <param name="id">The identifier which is the S3 key.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        /// <exception cref="GetObjectRequest">
        /// </exception>
        /// <exception cref="System.Exception">Failed to get blob [{res.HttpStatusCode}]</exception>
        public async ValueTask<Stream> GetStreamAsync(Env env, string id, CancellationToken cancellation = default)
        {
            string bucketName = GetBucket(env);
            string key = GetKey(env, id);
            var s3Request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };
            try
            {
                // s3Request.Headers.ExpiresUtc = DateTime.Now.AddHours(2); // cache expiration

                GetObjectResponse? res = await _client.GetObjectAsync(s3Request, cancellation);

                #region Validation

                if (res == null)
                {
                    throw new EventSourcingException($"S3 key [{key}] not found. bucket = {bucketName}");
                }

                if (res.HttpStatusCode >= HttpStatusCode.Ambiguous)
                {
                    throw new EventSourcingException($"Failed to get blob [{res.HttpStatusCode}]");
                }

                #endregion // Validation

                return res.ResponseStream;
            }
            #region Exception Handling

            catch (AmazonS3Exception e)
            {
                _logger.LogError(e.FormatLazy(),
                        "S3 Failed to get [{bucket}: {key}]: {message}", bucketName, key, e.Message);
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e.FormatLazy(),
                        "S3 get Failed: {bucket}: {key}", bucketName, key);
                throw;
            }

            #endregion // Exception Handling
        }

        #endregion // GetStreamAsync

        #region SaveAsync

        /// <summary>
        /// Saves content.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="env">Environment</param>
        /// <param name="id">The identifier of the resource.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Failed to save blob [{res.HttpStatusCode}]</exception>
        public async ValueTask<BlobResponse> SaveAsync(
            JsonElement data,
            Env env,
            string id,
            IImmutableDictionary<string, string>? metadata = null,
            IImmutableDictionary<string, string>? tags = null,
            CancellationToken cancellation = default)
        {
            var result = await SaveAsync(data.ToStream(), env, id, metadata, tags, "application/json", cancellation);
            return result;
        }

        /// <summary>
        /// Saves content.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="env">Environment</param>
        /// <param name="id">The identifier of the resource.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="mediaType">Type of the media.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Failed to save blob [{res.HttpStatusCode}]</exception>
        public async ValueTask<BlobResponse> SaveAsync(
            ReadOnlyMemory<byte> data,
            Env env,
            string id,
            IImmutableDictionary<string, string>? metadata = null,
            IImmutableDictionary<string, string>? tags = null,
            string? mediaType = null,
            CancellationToken cancellation = default)
        {
            using var srm = new MemoryStream(data.ToArray());
            var result = await SaveAsync(srm, env, id, metadata, tags, mediaType, cancellation);
            return result;
        }

        /// <summary>
        /// Saves content.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="env">Environment</param>
        /// <param name="id">The identifier of the resource.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="mediaType">Type of the media.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Failed to save blob [{res.HttpStatusCode}]</exception>
        /// <exception cref="Exception">Failed to save blob [{res.HttpStatusCode}]</exception>
        public async ValueTask<BlobResponse> SaveAsync(
            Stream data,
            Env env,
            string id,
            IImmutableDictionary<string, string>? metadata = null,
            IImmutableDictionary<string, string>? tags = null,
            string? mediaType = null,
            CancellationToken cancellation = default)
        {
            var bucket = GetBucket(env);
            string key = GetKey(env, id);
            try
            {
                var date = DateTime.UtcNow;
                //tags = tags.Add("month", date.ToString("yyyy-MM"));

                var s3Request = new PutObjectRequest
                {
                    BucketName = bucket,
                    Key = key,
                    InputStream = data,
                    ContentType = mediaType,
                    TagSet = tags?.Select(m => new Tag { Key = m.Key, Value = m.Value })?.ToList() ?? EMPTY_TAGS,
                };
                // s3Request.Headers.ExpiresUtc = DateTime.Now.AddHours(2); // cache expiration

                if (metadata != null)
                {
                    foreach (var meta in metadata)
                    {
                        s3Request.Metadata.Add(meta.Key, meta.Value);
                    }
                }

                #region if (_dryRun) return ...

                if (_dryRun)
                {
                    return new BlobResponse(key, string.Empty, string.Empty);
                }

                #endregion // if (_dryRun) return ...

                PutObjectResponse res = await _client.PutObjectAsync(s3Request, cancellation);

                #region Validation

                if (res.HttpStatusCode >= HttpStatusCode.Ambiguous)
                {
                    throw new EventSourcingException($"Failed to save blob [{res.HttpStatusCode}]");
                }

                #endregion // Validation

                BlobResponse response = new BlobResponse(key, res.ETag, res.VersionId);
                return response;
            }
            #region Exception Handling

            catch (AmazonS3Exception e)
            {
                string json = "";
                try
                {
                    json = data.Serialize();
                }
                catch { }
                _logger.LogError(e.FormatLazy(),
                        "AWS-S3 Failed to write: {payload}, {env}, {id}, {bucket}, {key}", json, env, id, bucket, key);
                string msg = $"AWS-S3 Failed to write: {env}, {id}, {bucket}, {key}";
                throw new ApplicationException(msg, e);
            }
            catch (Exception e)
            {
                string json = "";
                try
                {
                    json = data.Serialize();
                }
                catch { }
                _logger.LogError(e.FormatLazy(),
                        "S3 writing Failed: {payload}, {env}, {id}, {bucket}, {key}", json, env, id, bucket, key);
                string msg = $"S3 writing Failed: {env}, {id}, {bucket}, {key}";
                throw new ApplicationException(msg, e);
            }

            #endregion // Exception Handling
        }

        #endregion // SaveAsync

        #region GetBucket

        /// <summary>
        /// Get the Bucket name
        /// </summary>
        /// <param name="env">Environment</param>
        /// <returns></returns>
        private string GetBucket(Env env)
        {
            var bucket = _environmentConvension switch
            {
                S3EnvironmentConvention.BucketPrefix => $"{env.Format()}.{_bucket}",
                _ => _bucket
            };
            return bucket;
        }

        #endregion // GetBucket

        #region GetKey

        /// <summary>
        /// Gets s3 key.
        /// </summary>
        /// <param name="env">Environment</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        private string GetKey(string env, string id)
        {
            string sep = string.IsNullOrEmpty(_basePath) ? string.Empty : "/";
            string key = $"{_basePath}{sep}{Uri.UnescapeDataString(id)}";
            if (_environmentConvension == S3EnvironmentConvention.PathPrefix)
                key = $"{env}/{key}";
            return key;
        }

        #endregion // GetKey

        #region Dispose Pattern

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (disposing && Interlocked.Decrement(ref _disposeCount) > 0) return;
            _client.Dispose();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="S3RepositoryFactory"/> class.
        /// </summary>
        ~S3Repository()
        {
            Dispose(false);
        }

        #endregion // Dispose Pattern
    }
}
