namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Origin environment of the message
    /// </summary>
    public interface IProducerBuilderEnvironment<T>
    {
        /// <summary>
        /// Origin environment of the message
        /// </summary>
        /// <param name="environment">The environment (null: keep current environment, empty: reset the environment to nothing).</param>
        /// <returns></returns>
        T Environment(Env? environment);
    }
}
