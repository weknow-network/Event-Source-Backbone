using System;
using System.Collections.Immutable;

namespace Weknow.EventSource.Backbone
{
    public class EventSourceOptions
    {
        public EventSourceOptions(IDataSerializer serializer)
        {
            Serializer = serializer;
        }

        public IDataSerializer Serializer { get; }
    }
}