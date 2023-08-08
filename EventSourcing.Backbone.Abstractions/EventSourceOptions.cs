using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Options
    /// </summary>
    public record EventSourceOptions
    {
        public static readonly EventSourceOptions Default;
        private static readonly IDataSerializer DEFAULT_SERIALIZER;
        internal static readonly JsonStringEnumConverter EnumConvertor = new JsonStringEnumConverter(JsonNamingPolicy.CamelCase);
        public static readonly JsonSerializerOptions SerializerOptions;

        static EventSourceOptions()
        {
            var serializerOptions = new JsonSerializerOptions
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
            SerializerOptions = serializerOptions;
            DEFAULT_SERIALIZER = new JsonDataSerializer(serializerOptions);
            Default = new EventSourceOptions();
        }

        /// <summary>
        /// Gets the telemetry level.
        /// </summary>
        public TelemetryLevel TelemetryLevel { get; init; } = TelemetryLevel.Default;

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        public IDataSerializer Serializer { get; init; } = DEFAULT_SERIALIZER;
    }
}