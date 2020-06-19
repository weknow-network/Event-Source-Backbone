using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
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
        /// <param name="intercept">The intercept.</param>
        /// <returns></returns>
        IEventSourceConsumer2Builder AddInterceptor(
            Action<AnnouncementRaw, ReadOnlyMemory<byte>> intercept);
        /// <summary>
        /// Adds the raw interceptor.
        /// Intercept the consumer side execution before de-serialization.
        /// </summary>
        /// <param name="interceptor">The intercept.</param>
        /// <returns></returns>
        IEventSourceProducer2Builder AddInterceptor(
                                IConsumerRawInterceptor interceptor);

        /// <summary>
        /// Adds the raw interceptor.
        /// Intercept the consumer side execution before de-serialization.
        /// </summary>
        /// <param name="intercept">The intercept.</param>
        /// <returns></returns>
        IEventSourceConsumer2Builder AddAsyncInterceptor(
            Func<AnnouncementRaw, ReadOnlyMemory<byte>, Task> intercept);
        /// <summary>
        /// Adds the raw interceptor.
        /// Intercept the consumer side execution before de-serialization.
        /// </summary>
        /// <param name="interceptor">The intercept.</param>
        /// <returns></returns>
        IEventSourceProducer2Builder AddAsyncInterceptor(
                                IConsumerRawAsyncInterceptor interceptor);

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
