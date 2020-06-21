using System;
using System.Collections.Immutable;

namespace Weknow.EventSource.Backbone
{
    public class EventSourceOptions
    {
        public EventSourceOptions(
            IDataSerializer? serializer = null,
            bool useFullName = false)
        {
            Serializer = serializer ?? throw new NotImplementedException();
            UseFullName = useFullName;
        }

        public IDataSerializer Serializer { get; }
        public bool UseFullName { get; }
    }
}