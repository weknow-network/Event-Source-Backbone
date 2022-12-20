using System.Collections.Immutable;
using System.Text.Json;

namespace Weknow.EventSource.Backbone.Channels
{
    /// <summary>
    /// The S3 repository contract.
    /// </summary>
    public interface IS3Repository
    {
        /// <summary>
        /// Gets.
        /// </summary>
        /// <param name="env">The env.</param>
        /// <param name="id">The id.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>A ValueTask.</returns>
        ValueTask<T> GetAsync<T>(Env env, string id, CancellationToken cancellation = default);
        /// <summary>
        /// Gets json.
        /// </summary>
        /// <param name="env">The env.</param>
        /// <param name="id">The id.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>A ValueTask.</returns>
        ValueTask<JsonElement> GetJsonAsync(Env env, string id, CancellationToken cancellation = default);
        /// <summary>
        /// Gets stream.
        /// </summary>
        /// <param name="env">The env.</param>
        /// <param name="id">The id.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>A ValueTask.</returns>
        ValueTask<Stream> GetStreamAsync(Env env, string id, CancellationToken cancellation = default);
        /// <summary>
        /// Gets bytes.
        /// </summary>
        /// <param name="env">The env.</param>
        /// <param name="id">The id.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>A ValueTask.</returns>
        ValueTask<byte[]> GetBytesAsync(Env env, string id, CancellationToken cancellation = default);
        /// <summary>
        /// Save.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="env">The env.</param>
        /// <param name="id">The id.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>A ValueTask.</returns>
        ValueTask<BlobResponse> SaveAsync(JsonElement data, Env env, string id, IImmutableDictionary<string, string>? metadata = null, IImmutableDictionary<string, string>? tags = null, CancellationToken cancellation = default);
        /// <summary>
        /// Save.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="env">The env.</param>
        /// <param name="id">The id.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="mediaType">The media type.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>A ValueTask.</returns>
        ValueTask<BlobResponse> SaveAsync(ReadOnlyMemory<byte> data, Env env, string id, IImmutableDictionary<string, string>? metadata = null, IImmutableDictionary<string, string>? tags = null, string? mediaType = null, CancellationToken cancellation = default);
        /// <summary>
        /// Save.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="env">The env.</param>
        /// <param name="id">The id.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="mediaType">The media type.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>A ValueTask.</returns>
        ValueTask<BlobResponse> SaveAsync(Stream data, Env env, string id, IImmutableDictionary<string, string>? metadata = null, IImmutableDictionary<string, string>? tags = null, string? mediaType = null, CancellationToken cancellation = default);
    }
}