using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

using Microsoft.Extensions.Logging;


namespace Weknow.EventSource.Backbone.Channels
{
    /// <summary>
    /// Constants
    /// </summary>
    public static class Constants
    {
        public const string PROVIDER_ID = "S3_V1";

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
                            JsonImmutableDictionaryConverter.Default
                        }
        };
    }

}
