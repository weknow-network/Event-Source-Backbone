using System;

namespace Weknow.EventSource.Backbone
{
    public interface IEventSourceOptions
    {
        IDataSerializer Serializer { get; }
        bool UseFullName { get; }
    }
}