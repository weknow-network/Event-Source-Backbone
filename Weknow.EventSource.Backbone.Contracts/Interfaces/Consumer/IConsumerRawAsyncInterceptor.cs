using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    public interface IConsumerRawAsyncInterceptor
    {
         Task InterceptAsync(
                    AnnouncementMetadata metadata,
                    ReadOnlyMemory<byte> data);
    }
}