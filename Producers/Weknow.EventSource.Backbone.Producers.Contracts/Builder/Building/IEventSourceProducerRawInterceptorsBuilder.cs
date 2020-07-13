namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IEventSourceProducerRawInterceptorsBuilder:
        IEventSourceProducerSpecializeBuilder
    {
        /// <summary>
        /// Adds Producer interceptor (stage = after serialization).
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns></returns>
        IEventSourceProducerRawInterceptorsBuilder AddInterceptor(
                                IProducerRawInterceptor interceptor);

        /// <summary>
        /// Adds Producer interceptor (stage = after serialization).
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns></returns>
        IEventSourceProducerRawInterceptorsBuilder AddAsyncInterceptor(
                                IProducerRawAsyncInterceptor interceptor);

    }
}
