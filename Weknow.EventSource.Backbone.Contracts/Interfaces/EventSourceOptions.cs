using System;
using System.Collections.Immutable;

namespace Weknow.EventSource.Backbone
{
    public class EventSourceOptions
    {
        public static readonly EventSourceOptions Empty = new EventSourceOptions();

        public EventSourceOptions(
            IDataSerializer? serializer = null,
            bool useFullName = false)
        {
            Serializer = serializer ?? throw new NotImplementedException(); // TODO: json implementation
            UseFullName = useFullName;
        }

        public IDataSerializer Serializer { get; }
        public bool UseFullName { get; }
    }
}