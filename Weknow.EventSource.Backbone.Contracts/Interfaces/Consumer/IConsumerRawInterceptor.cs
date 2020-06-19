using System;
using System.Collections.Immutable;

namespace Weknow.EventSource.Backbone
{
    public interface IConsumerRawInterceptor
    {
         void Intercept(
                    AnnouncementMetadata metadata,
                    ReadOnlyMemory<byte> data);
    }
}