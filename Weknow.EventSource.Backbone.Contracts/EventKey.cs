﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

using static Weknow.Text.Json.Constants;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Event Id
    /// </summary>
    public class EventKey
    {
        private readonly string? _strId;
        private readonly JsonElement? _jsonId;
        private static readonly JsonStringEnumConverter EnumConvertor = new JsonStringEnumConverter(JsonNamingPolicy.CamelCase);

        private static readonly JsonSerializerOptions JSON_OPTIONS = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            Converters = { EnumConvertor, JsonImmutableDictionaryConverter.Default }
        };

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public EventKey(string id)
        {
            _strId = id;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public EventKey(JsonElement id)
        {
            _jsonId = id;
        }

        #endregion // Ctor

        #region ToString

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (_strId != null) return _strId;
            return _jsonId.Serialize(JSON_OPTIONS);
        }

        #endregion // ToString

        #region Cast overloads

        /// <summary>
        /// Performs an implicit conversion.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator string(EventKey id) => id.ToString();

        /// <summary>
        /// Performs an implicit conversion from <see cref="EventKey"/> to <see cref="JsonElement"/>.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator JsonElement(EventKey id)
        {
            var json = id._jsonId;
            if (json != null) return json ?? default;

            string str = id._strId ?? string.Empty;
            // TODO: [bnaya 2021] ask Avi
            if (str[0] != '{')
                str = $"{{\"value\":\"{str}\"}}";
            var result = JsonSerializer.Deserialize<JsonElement>(str, JSON_OPTIONS);
            return result;
        }


        #endregion // Cast overloads
    }
}
