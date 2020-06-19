using System;
using System.Collections.Immutable;

namespace Weknow.EventSource.Backbone.Building
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