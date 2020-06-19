using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    public interface IProducerRawAsyncInterceptor
    {
         ValueTask<(string key, ReadOnlyMemory<byte>)> InterceptAsync(
                                AnnouncementMetadata metadata);
    }
}