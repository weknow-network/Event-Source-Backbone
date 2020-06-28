namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IEventSourceProducer2Builder
    {
        /// <summary>
        /// Adds Producer interceptor (stage = after serialization).
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns></returns>
        IEventSourceProducer2Builder AddInterceptor(
                                IProducerRawInterceptor interceptor);

        /// <summary>
        /// Adds Producer interceptor (stage = after serialization).
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns></returns>
        IEventSourceProducer2Builder AddAsyncInterceptor(
                                IProducerRawAsyncInterceptor interceptor);

        /// <summary>
        /// Define the Producer for payload type and default eventName.
        /// </summary>
        /// <typeparam name="T">The payload type</typeparam>
        /// <param name="defaultEventName">The event name is the operation key.
        /// It can stand for itself for simple event or be associate with typed payload.
        /// It's recommended not to use the payload type as the event name, 
        /// because the payload type should be change on each breaking change of the type
        /// in order to support multi versions.
        /// </param>
        /// <returns></returns>
        IEventSourceProducer3Builder<T> ForEventType<T>(string defaultEventName) 
                                        where T: notnull; 
        /// <summary>
        /// Define the Producer with default eventName.
        /// </summary>
        /// <param name="defaultEventName">The event name is the operation key.</param>
        /// <returns></returns>
        IEventSourceProducer3Builder<string> ForEventType(string defaultEventName); 
    }
}
