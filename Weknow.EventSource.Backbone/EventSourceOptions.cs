using System;

namespace Weknow.EventSource.Backbone
{
    public class EventSourceOptions: IEventSourceOptions
    {
        private static readonly IDataSerializer DEFAULT_SERIALIZER = new JsonDataSerializer();
        public static readonly EventSourceOptions Empty = new EventSourceOptions();

        public EventSourceOptions(
            IDataSerializer? serializer = null,
            bool useFullName = false)
        {
            Serializer = serializer ?? DEFAULT_SERIALIZER;
            UseFullName = useFullName;
        }

        public IDataSerializer Serializer { get; }
        public bool UseFullName { get; }
    }
}