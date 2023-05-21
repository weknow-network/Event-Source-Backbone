using System.Diagnostics;

using Microsoft.Extensions.Logging;

namespace EventSourcing.Backbone.Private
{
    /// <summary>
    /// Best practice is to supply proper logger and 
    /// not using this class.Default logger.
    /// This class use Trace logger just in case the other logger is missing.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Logging.ILogger" />
    /// <seealso cref="System.IDisposable" />
    public sealed class EventSourceFallbakLogger : ILogger, IDisposable
    {
        #region Default

        /// <summary>
        /// The default
        /// </summary>
        public static readonly ILogger Default = new EventSourceFallbakLogger();

        #endregion // Default

        #region BeginScope

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>
        /// An <see cref="T:System.IDisposable" /> that ends the logical operation scope on dispose.
        /// </returns>
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => this;

        #endregion // BeginScope

        #region IsEnabled

        /// <summary>
        /// Checks if the given <paramref name="logLevel" /> is enabled.
        /// </summary>
        /// <param name="logLevel">level to be checked.</param>
        /// <returns>
        ///   <c>true</c> if enabled.
        /// </returns>
        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        #endregion // IsEnabled

        /// <summary>
        /// Writes a log entry.
        /// </summary>
        /// <typeparam name="TState">The type of the object to be written.</typeparam>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">Id of the event.</param>
        /// <param name="state">The entry to be written. Can be also an object.</param>
        /// <param name="exception">The exception related to this entry.</param>
        /// <param name="formatter">Function to create a <see cref="T:System.String" /> message of the <paramref name="state" /> and <paramref name="exception" />.</param>
        /// <exception cref="NotImplementedException"></exception>
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        public void Log<TState>(
            LogLevel logLevel,
            Microsoft.Extensions.Logging.EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            string message = string.Empty;
            if (exception == null)
            {
                message = state?.ToString() ?? string.Empty;
            }
            else
            {
                message = formatter(state, exception);
            }
            Trace.WriteLine(message, logLevel.ToString());
        }
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).

        #region Dispose

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() { }

        #endregion // Dispose
    }
}
