namespace EventSourcing.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IConsumerEnvironmentOfBuilder<out T>
    {
        /// <summary>
        /// Include the environment as prefix of the stream key.
        /// for example: env:URI
        /// </summary>
        /// <param name="environment">The environment (null: keep current environment, empty: reset the environment to nothing).</param>
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
