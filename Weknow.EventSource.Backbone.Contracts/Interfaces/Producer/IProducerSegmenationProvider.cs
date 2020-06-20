using System;
using System.Collections.Immutable;

namespace Weknow.EventSource.Backbone.Building
{
    public interface IProducerSegmenationProvider<T> 
                                    where T : notnull
    {
        ImmutableDictionary<string, ReadOnlyMemory<byte>> Classify(T data, IDataSerializer serializer);
    }
}