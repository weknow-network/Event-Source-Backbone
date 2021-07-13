using System;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.Channels
{
    public interface IS3Repository
    {
        ValueTask<T> GetAsync<T>(string id, CancellationToken cancellation = default);
        ValueTask<JsonElement> GetJsonAsync(string id, CancellationToken cancellation = default);
        ValueTask<Stream> GetStreamAsync(string id, CancellationToken cancellation = default);
        ValueTask<byte[]> GetBytesAsync(string id, CancellationToken cancellation = default);
        ValueTask<BlobResponse> SaveAsync(JsonElement data, string id, IImmutableDictionary<string, string>? metadata = null, IImmutableDictionary<string, string>? tags = null,  CancellationToken cancellation = default);
        ValueTask<BlobResponse> SaveAsync(ReadOnlyMemory<byte> data, string id, IImmutableDictionary<string, string>? metadata = null, IImmutableDictionary<string, string>? tags = null, string? mediaType = null, CancellationToken cancellation = default);
        ValueTask<BlobResponse> SaveAsync(Stream data, string id, IImmutableDictionary<string, string>? metadata = null, IImmutableDictionary<string, string>? tags = null, string? mediaType = null, CancellationToken cancellation = default);
    }
}