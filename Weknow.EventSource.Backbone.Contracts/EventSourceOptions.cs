namespace Weknow.EventSource.Backbone
{
    public class EventSourceOptions: IEventSourceOptions
    {
        private static readonly IDataSerializer DEFAULT_SERIALIZER = new JsonDataSerializer();
        public static readonly EventSourceOptions Empty = new EventSourceOptions();

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        public EventSourceOptions(
            IDataSerializer? serializer = null)
        {
            Serializer = serializer ?? DEFAULT_SERIALIZER;
        }

        #endregion // Ctor

        #region Serializer

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        public IDataSerializer Serializer { get; }

        #endregion // Serializer
    }
}