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
    /// <typeparam name="T">The type of the sending data</typeparam>
    public interface IEventSourceProducer3Builder<T>
        where T : notnull
    {
        IEventSourceProducer3Builder<T> AddInterceptor(
            Func<AnnouncementMetadata, T, (string key, string value)> intercept);
        IEventSourceProducer3Builder<T> AddAsyncInterceptor(
            Func<AnnouncementMetadata, T, ValueTask<(string key, string value)>> intercept);
       
        IEventSourceProducer4Builder<T> AddSegmenationProvider(
            ISegmenationProducerProvider<T> segmentationProvider);
        IEventSourceProducer4Builder<T> AddSegmenationProvider(
            Func<T, 
                IDataSerializer,
                ImmutableDictionary<string, ReadOnlyMemory<byte>>> segmentationProvider);
    }
}
