using System;
using System.Collections.Immutable;

namespace Weknow.EventSource.Backbone.Building
{
    public interface IDataSerializer
    {
        ReadOnlyMemory<byte> Serialize<T>(T item);
        T Deserialize<T>(ReadOnlyMemory<byte> serializedData);
    }
}