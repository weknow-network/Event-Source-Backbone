namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Base class for hosted services (workers) which 
    /// help with best practice implementation of the host.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Hosting.IHostedService" />
    public abstract class HostedServiceBase : IHostedService
    {
        /// <summary>
        /// The logger
        /// </summary>
        protected readonly ILogger _logger;
        private Task? _running;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="HostedServiceBase"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public HostedServiceBase(
            ILogger logger)
        {
            _logger = logger;
        }

        #endregion // Ctor

        #region StartAsync

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        /// <returns></returns>
        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            _running = OnStartAsync(cancellationToken);
            return Task.CompletedTask;
        }

        #endregion // StartAsync

        #region OnStartAsync

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        protected abstract Task OnStartAsync(CancellationToken cancellationToken);

        #endregion // OnStartAsync

        #region StopAsync

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        /// <returns></returns>
        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return _running ?? Task.CompletedTask;
        }

        #endregion // StopAsync
    }
}
