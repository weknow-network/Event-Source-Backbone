using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    public interface IProducerAsyncInterceptor<T>
        where T: notnull
    {
         ValueTask<(string key, ReadOnlyMemory<byte>)> Intercept(
                                AnnouncementMetadata metadata, T payload);
    }
}