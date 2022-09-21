using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using static Weknow.EventSource.Backbone.EventSourceConstants;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Bucket convertor
    /// </summary>
    /// <seealso cref="System.Text.Json.Serialization.JsonConverter&lt;Weknow.EventSource.Backbone.Bucket&gt;" />
    public class BucketJsonConverter : JsonConverter<Bucket>
    {
        public static readonly BucketJsonConverter Instance = new BucketJsonConverter();

        #region Crot (hidden)

        /// <summary>
        /// Prevents a default instance of the <see cref="BucketJsonConverter"/> class from being created.
        /// </summary>
        private BucketJsonConverter() { }

        #endregion // Crot (hidden)

        #region Read

        /// <summary>
        /// Reads and converts the JSON to type <typeparamref name="T" />.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        /// <exception cref="System.IO.InvalidDataException">
        /// Unexpected empty property key in {nameof(BucketJsonConverter)}
        /// or
        /// Unexpected empty property [{k}] value in {nameof(BucketJsonConverter)}
        /// </exception>
        public override Bucket Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var bucket = Bucket.Empty;

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                string? k = reader.GetString() ?? throw new InvalidDataException($"Unexpected empty property key in {nameof(BucketJsonConverter)}");
                reader.Read();
                byte[] value = JsonSerializer.Deserialize<byte[]>(ref reader, options) ?? throw new InvalidDataException($"Unexpected empty property [{k}] value in {nameof(BucketJsonConverter)}");

                bucket = bucket.Add(k, value);
            }
            return bucket;
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
            writer.WriteStartObject();
            foreach (var pair in bucket)
            {
                writer.WritePropertyName(pair.Key);
                JsonSerializer.Serialize(writer, pair.Value.ToArray(), options);
            }
            writer.WriteEndObject();
        }
        #endregion // Write
    }
}