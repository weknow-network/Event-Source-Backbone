using System;
using System.Collections.Immutable;

namespace Weknow.EventSource.Backbone
{
    public interface IProducerInterceptor<T>
        where T: notnull
    {
         (string key, ReadOnlyMemory<byte>) Intercept(
                                AnnouncementMetadata metadata, T payload);
    }
}