using System;
using System.Collections.Immutable;

namespace Weknow.EventSource.Backbone.Building
{
    public interface IConsumerSegmenationProvider<T> where T : notnull
    {
        T  Unclassify(ImmutableDictionary<string, ReadOnlyMemory<byte>> segments, IDataSerializer serializer);
    }
}