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
        IEventSourceConsumer2Builder AddInterceptor(
            Func<AnnouncementRaw, (string key, string value)> intercept);

        IEventSourceConsumer3Builder<T> ForType<T>(
                        ISegmenationConsumerProvider<T> segmentationProvider,
                        params string[] intents)
                     where T : notnull;

        IEventSourceConsumer3Builder<T> ForType<T>(
                        Func<ImmutableDictionary<string, ReadOnlyMemory<byte>>,
                            IDataSerializer,
                            T> segmentationProvider,
                        params string[] intents)
                     where T : notnull;

        IEventSourceConsumer3Builder<T> ForType<T>(
                        ISegmenationConsumerProvider<T> segmentationProvider,
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
