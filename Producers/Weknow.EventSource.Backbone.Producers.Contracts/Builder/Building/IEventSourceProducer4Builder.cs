namespace Weknow.EventSource.Backbone.Building
{

    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    /// <typeparam name="T">The type of the sending data</typeparam>
    public interface IEventSourceProducer4Builder<T>
        where T : notnull
    {
        /// <summary>
        /// Builds producer instance.
        /// </summary>
        /// <returns></returns>
        IEventSourceProducer<T> Build();
    }
}
