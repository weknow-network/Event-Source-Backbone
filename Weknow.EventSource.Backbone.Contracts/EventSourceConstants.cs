﻿using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Constants
    /// </summary>
    public static class EventSourceConstants
    {
        /// <summary>
        /// The name of redis consumer channel source
        /// </summary>
        public const string REDIS_CONSUMER_CHANNEL_SOURCE = "redis-consumer-channel";
        /// <summary>
        /// The name of redis producer channel source
        /// </summary>
        public const string REDIS_PRODUCER_CHANNEL_SOURCE = "redis-producer-channel";

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
                            JsonDictionaryConverter.Default,
                            JsonImmutableDictionaryConverter.Default,
                            JsonMemoryBytesConverterFactory.Default
                        }
        };
    }
}
