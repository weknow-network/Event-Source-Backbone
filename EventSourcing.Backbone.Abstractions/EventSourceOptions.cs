using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Options
    /// </summary>
    public record EventSourceOptions
    {
        private static readonly IDataSerializer DEFAULT_SERIALIZER = new JsonDataSerializer(FullSerializerOptions);
        private static readonly JsonStringEnumConverter EnumConvertor = new JsonStringEnumConverter(JsonNamingPolicy.CamelCase);

        #region TelemetryLevel

        /// <summary>
        /// Gets the telemetry level.
        /// </summary>
        public TelemetryLevel TelemetryLevel { get; init; } = TelemetryLevel.Default;

        #endregion // TelemetryLevel

        #region SerializerOptions

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

        #endregion // SerializerOptions

        #region FullSerializerOptions

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

        #endregion // FullSerializerOptions

        #region Serializer

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        public IDataSerializer Serializer { get; init; } = DEFAULT_SERIALIZER;

        #endregion // Serializer
    }
}