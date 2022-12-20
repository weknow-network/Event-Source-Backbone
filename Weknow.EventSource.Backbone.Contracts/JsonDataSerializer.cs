using System.Text;
using System.Text.Json;

using static Weknow.Text.Json.Constants;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Json serializer (this is the default serializer)
    /// </summary>
    /// <seealso cref="Weknow.EventSource.Backbone.IDataSerializer" />
    internal class JsonDataSerializer : IDataSerializer
    {
        private readonly JsonSerializerOptions _options;

        public JsonDataSerializer(JsonSerializerOptions? options = null)
        {
            _options = options ?? SerializerOptions;
        }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
        T IDataSerializer.Deserialize<T>(ReadOnlyMemory<byte> serializedData)
        {
            T result = JsonSerializer.Deserialize<T>(serializedData.Span, _options);
            return result;
        }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8603 // Possible null reference return.

        ReadOnlyMemory<byte> IDataSerializer.Serialize<T>(T item)
        {
            string result = JsonSerializer.Serialize(item, _options);
            return Encoding.UTF8.GetBytes(result).AsMemory();
        }
    }
}