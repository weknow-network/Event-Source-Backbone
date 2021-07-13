using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

using Microsoft.Extensions.Logging;

using static Weknow.EventSource.Backbone.Channels.Constants;

namespace Weknow.EventSource.Backbone.Channels
{
    /// <summary>
    /// Abstract S3 operations
    /// </summary>
    public sealed class S3Repository : IS3Repository
    {
        private readonly string _bucket;
        private readonly ILogger _logger;
        private readonly string? _basePath;
        private readonly AmazonS3Client _client;
        private static readonly ImmutableHashSet<string> VALID_HEADERS = ImmutableHashSet<string>.Empty
                                        .Add("Content-Type");
        private static readonly List<Tag> EMPTY_TAGS = new List<Tag>();

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="client">S3 client.</param>
        /// <param name="bucket">The s3 bucket.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="basePath">The base path within the bucket.</param>
        public S3Repository(
                    AmazonS3Client client,
                    string bucket,
                    ILogger logger,
                    string? basePath = null)
        {
            _client = client;
            _bucket = bucket;
            _logger = logger;
            _basePath = basePath;
        }

        #endregion // Ctor

        #region GetJsonAsync

        /// <summary>
        /// Get content.
        /// </summary>
        /// <param name="id">The identifier which is the S3 key.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.InvalidDataException"></exception>
        public async ValueTask<JsonElement> GetJsonAsync(string id, CancellationToken cancellation = default)
        {
            try
            {
                Stream srm = await GetStreamAsync(id, cancellation);

                var response = await JsonDocument.ParseAsync(srm);
                return response.RootElement;
            }
            #region Exception Handling

            catch (Exception e)
            {
                string msg = "S3 Failed to parse json:";
                _logger.LogError(e.FormatLazy(), msg);
                throw new InvalidDataException();
            }

            #endregion // Exception Handling
        }

        #endregion // GetJsonAsync

        #region GetJsonAsync

        /// <summary>
        /// Get content.
        /// </summary>
        /// <param name="id">The identifier which is the S3 key.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.InvalidDataException"></exception>
        public async ValueTask<byte[]> GetBytesAsync(string id, CancellationToken cancellation = default)
        {
            try
            {
                Stream srm = await GetStreamAsync(id, cancellation);
                var buffer = new byte[srm.Length];
                await srm.ReadAsync(buffer, cancellation);
                return buffer;
            }
            #region Exception Handling

            catch (Exception e)
            {
                string msg = "S3 Failed to parse json:";
                _logger.LogError(e.FormatLazy(), msg);
                throw new InvalidDataException();
            }

            #endregion // Exception Handling
        }

        #endregion // GetJsonAsync

        #region GetAsync


        /// <summary>
        /// Get content.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier which is the S3 key.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        /// <exception cref="System.NullReferenceException">Failed to deserialize industries</exception>
        /// <exception cref="System.IO.InvalidDataException"></exception>
        public async ValueTask<T> GetAsync<T>(string id, CancellationToken cancellation = default)
        {
            try
            {
                Stream srm = await GetStreamAsync(id, cancellation);

                var response = await JsonSerializer.DeserializeAsync<T>(srm, SerializerOptionsWithIndent);

                #region Validation

                if (response == null)
                {
                    throw new NullReferenceException("Failed to deserialize industries");
                }

                #endregion // Validation
                return response;
            }
            #region Exception Handling

            catch (Exception e)
            {
                string msg = $"S3 Failed to deserialize into [{typeof(T).Name}]:";
                _logger.LogError(e.FormatLazy(), msg);
                throw new InvalidDataException();
            }

            #endregion // Exception Handling
        }

        #endregion // GetAsync

        #region GetStreamAsync

        /// <summary>
        /// Get content.
        /// </summary>
        /// <param name="id">The identifier which is the S3 key.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        /// <exception cref="GetObjectRequest">
        /// </exception>
        /// <exception cref="System.Exception">Failed to get blob [{res.HttpStatusCode}]</exception>
        public async ValueTask<Stream> GetStreamAsync(string id, CancellationToken cancellation = default)
        {
            try
            {
                string key = GetKey(id);
                var s3Request = new GetObjectRequest
                {
                    BucketName = _bucket,
                    Key = key
                };
                // s3Request.Headers.ExpiresUtc = DateTime.Now.AddHours(2); // cache expiration

                GetObjectResponse res = await _client.GetObjectAsync(s3Request, cancellation);

                #region Validation

                if (res.HttpStatusCode >= HttpStatusCode.Ambiguous)
                {
                    throw new Exception($"Failed to get blob [{res.HttpStatusCode}]");
                }

                #endregion // Validation

                return res.ResponseStream;
            }
            #region Exception Handling

            catch (AmazonS3Exception e)
            {
                _logger.LogError(e.FormatLazy(),
                        "S3 Failed to get:");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e.FormatLazy(),
                        "S3 get Failed");
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
        /// <param name="id">The identifier of the resource.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Failed to save blob [{res.HttpStatusCode}]</exception>
        public async ValueTask<BlobResponse> SaveAsync(
            JsonElement data,
            string id,
            IImmutableDictionary<string, string>? metadata = null,
            IImmutableDictionary<string, string>? tags = null,
            CancellationToken cancellation = default)
        {
            var result = await SaveAsync(data.ToStream(), id, metadata, tags, "application/json", cancellation);
            return result;
        }

        /// <summary>
        /// Saves content.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="id">The identifier of the resource.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="mediaType">Type of the media.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Failed to save blob [{res.HttpStatusCode}]</exception>
        public async ValueTask<BlobResponse> SaveAsync(
            ReadOnlyMemory<byte> data,
            string id,
            IImmutableDictionary<string, string>? metadata = null,
            IImmutableDictionary<string, string>? tags = null,
            string? mediaType = null,
            CancellationToken cancellation = default)
        {
            using var srm = new MemoryStream(data.ToArray()); // TODO: [bnaya 2021-07] consider AsStream -> https://www.nuget.org/packages/Microsoft.Toolkit.HighPerformance
            var result = await SaveAsync(srm, id, metadata, tags, mediaType, cancellation );
            return result;
        }

        /// <summary>
        /// Saves content.
        /// </summary>
        /// <param name="data">The data.</param>
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
            string id,
            IImmutableDictionary<string, string>? metadata = null,
            IImmutableDictionary<string, string>? tags = null,
            string? mediaType = null,
            CancellationToken cancellation = default)
        {
            try
            {
                var date = DateTime.UtcNow;
                string key = GetKey(id);
                //tags = tags.Add("month", date.ToString("yyyy-MM"));

                var s3Request = new PutObjectRequest
                {
                    BucketName = _bucket,
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

                PutObjectResponse res = await _client.PutObjectAsync(s3Request, cancellation);

                #region Validation

                if (res.HttpStatusCode >= HttpStatusCode.Ambiguous)
                {
                    throw new Exception($"Failed to save blob [{res.HttpStatusCode}]");
                }

                #endregion // Validation

                var response = new BlobResponse(key, _bucket, res.ETag, res.VersionId);
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
                        "AWS-S3 Failed to write: {payload}", json);
                throw;
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
                        "S3 writing Failed: {payload}", json);
                throw;
            }

            #endregion // Exception Handling
        }

        #endregion // SaveAsync

        #region GetKey

        /// <summary>
        /// Gets s3 key.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        private string GetKey(string id)
        {
            string sep = string.IsNullOrEmpty(_basePath) ? string.Empty : "/";
            string key = $"{_basePath}{sep}{Uri.UnescapeDataString(id)}";
            return key;
        }

        #endregion // GetKey
    }

}
