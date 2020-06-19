using System;
using System.Collections.Immutable;

namespace Weknow.EventSource.Backbone.Building
{
    public interface ISegmenationProducerProvider<T> where T : notnull
    {
        ImmutableDictionary<string, ReadOnlyMemory<byte>> Classify(T data, IDataSerializer serializer);
    }
}