using Microsoft.Extensions.Logging;

using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Weknow.EventSource.Backbone
{
    public static class ProducerExtensions
    {
        #region AddVoidStrategy

        /// <summary>
        /// Adds the void storage strategy i.e. the storage data ignored.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        /// <remarks>
        /// It can be useful when migrating a stream which should reuse the same storage as the source stream.
        /// In this case it's important to maintain the original metadata & message's id.
        /// </remarks>
        public static IProducerStoreStrategyBuilder AddVoidStrategy(
            this IProducerStoreStrategyBuilder builder)
        {
            var result = builder.AddStorageStrategy(Local);

            ValueTask<IProducerStorageStrategy> Local(ILogger logger)
            {
                return NoneStorageStrategy.Instance.ToValueTask();
            }            

            return result;
        }

        #region class NoneStorageStrategy : IProducerStorageStrategy

        /// <summary>
        /// Non strategy implementation
        /// </summary>
        /// <seealso cref="Weknow.EventSource.Backbone.IProducerStorageStrategy" />
        private class NoneStorageStrategy : IProducerStorageStrategy
        { 
            public static IProducerStorageStrategy Instance = new NoneStorageStrategy();

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
                return ImmutableDictionary<string, string>.Empty.ToValueTask<IImmutableDictionary<string, string>>();
            }
        }

        #endregion // class NoneStorageStrategy : IProducerStorageStrategy

        #endregion // AddVoidStrategy
    }
}
