using System.Text.Json;
using System.Text.Json.Serialization;

namespace Weknow.EventSource.Backbone
{
    public record EventSourceOptions
    {
        private static readonly JsonStringEnumConverter EnumConvertor = new JsonStringEnumConverter(JsonNamingPolicy.CamelCase);

        //private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        //{
        //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        //    // PropertyNameCaseInsensitive = true,
        //    // IgnoreNullValues = true,
        //    //WriteIndented = true,
        //    Converters =
        //                {
        //                    EnumConvertor,
        //                    JsonDictionaryConverter.Default,
        //                    JsonImmutableDictionaryConverter.Default
        //    }
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            // PropertyNameCaseInsensitive = true,
            // IgnoreNullValues = true,
            //WriteIndented = true,
            Converters =
                        {
                            EnumConvertor,
                            JsonDictionaryConverter.Default,
                            JsonImmutableDictionaryConverter.Default,
                            BucketJsonConverter.Instance
                        }
        };

        private static readonly IDataSerializer DEFAULT_SERIALIZER = new JsonDataSerializer(SerializerOptions);

        //public static readonly EventSourceOptions Empty = new EventSourceOptions();

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        public IDataSerializer Serializer { get; init; } = DEFAULT_SERIALIZER;
    }
}