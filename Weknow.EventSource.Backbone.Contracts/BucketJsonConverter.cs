using System.Text.Json;
using System.Text.Json.Serialization;

using static Weknow.EventSource.Backbone.EventSourceOptions;

using BucketData = System.Collections.Immutable.ImmutableDictionary<string, System.ReadOnlyMemory<byte>>;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// <![CDATA[Json Bucket converter]]>
    /// </summary>
    /// <seealso cref="System.Text.Json.Serialization.JsonConverter" />
    public class JsonBucketConverter : JsonConverter<Bucket>
    {
        public static readonly JsonConverter<Bucket> Default = new JsonBucketConverter();

        #region Ctor

        /// <summary>
        /// Prevents a default instance of the <see cref="JsonBucketConverter"/> class from being created.
        /// </summary>
        private JsonBucketConverter()
        {

        }

        #endregion // Ctor

        #region Read

        /// <summary>
        /// Reads and converts the JSON to type.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        /// <exception cref="JsonException"></exception>
        public override Bucket Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var data = JsonSerializer.Deserialize<BucketData>(ref reader, SerializerOptions);
            Bucket result = data ?? BucketData.Empty;
            return result;
        }

        #endregion // Read

        #region Write

        /// <summary>
        /// Writes the specified writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="bucket">The bucket.</param>
        /// <param name="options">The options.</param>
        public override void Write(
            Utf8JsonWriter writer,
            Bucket bucket,
            JsonSerializerOptions options)
        {
            BucketData data = bucket;
            JsonSerializer.Serialize(writer, data, SerializerOptions);
        }

        #endregion // Write

    }

}