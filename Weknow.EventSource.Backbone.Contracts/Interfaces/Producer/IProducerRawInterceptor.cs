using System;
using System.Collections.Immutable;

namespace Weknow.EventSource.Backbone
{
    public interface IProducerRawInterceptor
    {
         (string key, ReadOnlyMemory<byte>) Intercept(
                                AnnouncementMetadata metadata);
    }
}