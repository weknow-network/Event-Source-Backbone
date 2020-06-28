using System;
using System.Collections.Immutable;
using System.Threading.Tasks.Dataflow;

namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IEventSourceConsumer2Builder
    {
        /// <summary>
        /// Adds the raw interceptor.
        /// Intercept the consumer side execution before de-serialization.
        /// </summary>
        /// <param name="interceptorData">The interceptor data as the interceptor defined in the producer stage.</param>
        /// <returns></returns>
        IEventSourceConsumer2Builder AddInterceptor(
                                IConsumerRawInterceptor interceptorData);

        /// <summary>
        /// Adds the raw interceptor.
        /// Intercept the consumer side execution before de-serialization.
        /// </summary>
        /// <param name="interceptorData">The interceptor data as the interceptor defined in the producer stage.</param>
        /// <returns></returns>
        IEventSourceConsumer2Builder AddAsyncInterceptor(
                                IConsumerRawAsyncInterceptor interceptorData);

        IEventSourceConsumer3Builder<T> ForType<T>(
                        IConsumerSegmenationProvider<T> segmentationProvider,
                        params string[] intents)
                     where T : notnull;

        IEventSourceConsumer3Builder<T> ForType<T>(
                        Func<ImmutableDictionary<string, ReadOnlyMemory<byte>>,
                            IDataSerializer,
                            T> segmentationProvider,
                        params string[] intents)
                     where T : notnull;

        IEventSourceConsumer3Builder<T> ForType<T>(
                        IConsumerSegmenationProvider<T> segmentationProvider,
                        Func<AnnouncementMetadata, bool> filter)
                     where T : notnull;

        IEventSourceConsumer3Builder<T> ForType<T>(
                        Func<ImmutableDictionary<string, ReadOnlyMemory<byte>>,
                            IDataSerializer,
                            T> segmentationProvider,
                        Func<AnnouncementMetadata, bool> filter)
                     where T : notnull;

        /// <summary>
        /// Builds consumer for non-specialized announcements.
        /// This is perfect for scenarios like storing backups in blobs like S3.
        /// </summary>
        /// <returns></returns>
        ISourceBlock<Ackable<AnnouncementRaw>> BuildRaw();
    }
}
