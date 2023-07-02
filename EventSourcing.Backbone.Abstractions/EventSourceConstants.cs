using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventSourcing.Backbone;

/// <summary>
/// Constants
/// </summary>
public static class EventSourceConstants
{

    public static readonly JsonStringEnumConverter EnumConvertor = new JsonStringEnumConverter(JsonNamingPolicy.CamelCase);
    public static readonly JsonSerializerOptions SerializerOptionsWithIndent = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        // PropertyNameCaseInsensitive = true,
        // IgnoreNullValues = true,
        WriteIndented = true,
        Converters =
                    {
                        EnumConvertor,
                        JsonMemoryBytesConverterFactory.Default
                    }
    };

    public const string TELEMETRY_SOURCE = "evt-src";
    public const string REDIS_TELEMETRY_SOURCE = $"{EventSourceConstants.TELEMETRY_SOURCE}-redis";
}
