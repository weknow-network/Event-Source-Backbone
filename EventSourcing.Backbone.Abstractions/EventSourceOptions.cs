using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventSourcing.Backbone
{
    public record EventSourceOptions
    {
        private static readonly JsonStringEnumConverter EnumConvertor = new JsonStringEnumConverter(JsonNamingPolicy.CamelCase);

        internal static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            Converters =
                        {
                            EnumConvertor,
                            JsonMemoryBytesConverterFactory.Default
                        }
        };

        public static readonly JsonSerializerOptions FullSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            Converters =
                        {
                            EnumConvertor,
                            JsonMemoryBytesConverterFactory.Default,
                            JsonBucketConverter.Default
                        }
        };

        private static readonly IDataSerializer DEFAULT_SERIALIZER = new JsonDataSerializer(FullSerializerOptions);

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        public IDataSerializer Serializer { get; init; } = DEFAULT_SERIALIZER;
    }
}