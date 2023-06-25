namespace EventSourcing.Backbone.Building
{
    /// <summary>
    /// Origin environment of the message
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IProducerBuilderEnvironment<out T>
    {
        /// <summary>
        /// Origin environment of the message
        /// </summary>
        /// <param name="environment">
        /// The environment (null: keep current environment, 
        /// empty: reset the environment to nothing).</param>
        /// <returns></returns>
        T Environment(Env? environment);

        /// <summary>
        /// Fetch the origin environment of the message from an environment variable.
        /// </summary>
        /// <param name="environmentVariableKey">The environment variable key.</param>
        /// <returns></returns>
        T EnvironmentFromVariable(string environmentVariableKey = "ASPNETCORE_ENVIRONMENT");
    }
}
