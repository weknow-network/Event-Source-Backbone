using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using static Weknow.EventSource.Backbone.EventSourceConstants;

namespace Weknow.EventSource.Backbone
{
    public static class ProducerExtensions
    {
        #region AddVoidStrategy

        /// <summary>
        /// Adds the void strategy.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="providerPrefix">The provider prefix.</param>
        /// <returns></returns>
        public static IProducerStoreStrategyBuilder AddVoidStrategy(
            this IProducerStoreStrategyBuilder builder,
            string providerPrefix = "S3_V1")
        {
            var result = builder.AddStorageStrategy(Local);

            ValueTask<IProducerStorageStrategy> Local(ILogger logger)
            {
                IProducerStorageStrategy result = new VoidStorageStrategy(providerPrefix);
                return result.ToValueTask();
            }

            return result;
        }

        #region class NoneStorageStrategy : IProducerStorageStrategy

        /// <summary>
        /// Non strategy implementation
        /// </summary>
        /// <seealso cref="Weknow.EventSource.Backbone.IProducerStorageStrategy" />
        private class VoidStorageStrategy : IProducerStorageStrategy
        {
            private readonly string _providerPrefix;

            #region Ctor

            /// <summary>
            /// Initializes a new instance of the <see cref="VoidStorageStrategy"/> class.
            /// </summary>
            /// <param name="providerPrefix">The provider prefix.</param>
            public VoidStorageStrategy(string providerPrefix)
            {
                _providerPrefix = providerPrefix;
            }

            #endregion // Ctor

            /// <summary>
            /// Saves the bucket information.
            /// </summary>
            /// <param name="id">The identifier.</param>
            /// <param name="bucket">Either Segments or Interceptions.</param>
            /// <param name="type">The type.</param>
            /// <param name="meta">The metadata.</param>
            /// <param name="cancellation">The cancellation.</param>
            /// <returns>
            /// Array of metadata entries which can be used by the consumer side storage strategy, in order to fetch the data.
            /// </returns>
            ValueTask<IImmutableDictionary<string, string>> IProducerStorageStrategy.SaveBucketAsync(
                string id,
                Bucket bucket,
                EventBucketCategories type,
                Metadata meta,
                CancellationToken cancellation)
            {
                KeyValuePair<string, string>[] pairs = bucket
                    .Select(pair =>
                    {
                        string value = Encoding.UTF8.GetString(pair.Value.ToArray());
                        return KeyValuePair.Create(pair.Key, value);
                    }).ToArray();
                string json = JsonSerializer.Serialize(pairs, SerializerOptionsWithIndent);
                var result = ImmutableDictionary<string, string>.Empty.Add($"{_providerPrefix}~{type}", json);
                return result.ToValueTask<IImmutableDictionary<string, string>>();
                //ImmutableDictionary<string, string> result = bucket;
                //return result.ToValueTask<IImmutableDictionary<string, string>>();
            }
        }

        #endregion // class NoneStorageStrategy : IProducerStorageStrategy

        #endregion // AddVoidStrategy
    }
}
