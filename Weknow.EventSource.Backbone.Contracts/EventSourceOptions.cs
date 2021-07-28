namespace Weknow.EventSource.Backbone
{
    public record EventSourceOptions
    {
        private static readonly IDataSerializer DEFAULT_SERIALIZER = new JsonDataSerializer();
        //public static readonly EventSourceOptions Empty = new EventSourceOptions();

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        public IDataSerializer Serializer { get; init; } = DEFAULT_SERIALIZER;
    }
}