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
    public interface IEventSourceProducer3Builder<T>:
                                IEventSourceProducer4Builder<T>
                            where T : notnull
    {
        IEventSourceProducer3Builder<T> AddInterceptor(
                    IProducerInterceptor<T> intercept);
        IEventSourceProducer3Builder<T> AddInterceptor(
            Func<AnnouncementMetadata, T, (string key, ReadOnlyMemory<byte> value)> intercept);
        IEventSourceProducer3Builder<T> AddAsyncInterceptor(
                    IProducerAsyncInterceptor<T> intercept);
        IEventSourceProducer3Builder<T> AddAsyncInterceptor(
            Func<AnnouncementMetadata, T, ValueTask<(string key, ReadOnlyMemory<byte> value)>> intercept);
       
        IEventSourceProducer4Builder<T> AddSegmenationProvider(
            IProducerSegmenationProvider<T> segmentationProvider);
        IEventSourceProducer4Builder<T> AddSegmenationProvider(
            Func<T, 
                IDataSerializer,
                ImmutableDictionary<string, ReadOnlyMemory<byte>>> segmentationProvider);
    }
}
