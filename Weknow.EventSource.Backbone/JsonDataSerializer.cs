using System;
using System.Text;
using System.Text.Json;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Json serializer (this is the default serializer)
    /// </summary>
    /// <seealso cref="Weknow.EventSource.Backbone.IDataSerializer" />
    public class JsonDataSerializer : IDataSerializer
    {
        private static readonly JsonSerializerOptions DEFAULT_OPTIONS = new JsonSerializerOptions();

        private readonly JsonSerializerOptions _options;

        public JsonDataSerializer(JsonSerializerOptions? options = null)
        {
            _options = options ?? DEFAULT_OPTIONS;
        }

        T IDataSerializer.Deserialize<T>(ReadOnlyMemory<byte> serializedData)
        {
            T result = JsonSerializer.Deserialize<T>(serializedData.Span, _options);
            return result;
        }

        ReadOnlyMemory<byte> IDataSerializer.Serialize<T>(T item)
        {
            string result = JsonSerializer.Serialize(item, _options);
            return Encoding.UTF8.GetBytes(result).AsMemory();
        }
    }
}