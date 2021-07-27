using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Hosting
{
    /// <summary>
    /// Weknow core extensions for ASP.NET Core
    /// </summary>
    public class WeknowHosting<TSetup>
        where TSetup : class
    {
        #region HostRest

        /// <summary>
        /// Starts the rest hosting.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="extensionPoint">The extension point.</param>
        public void HostRest(string[] args, Action<IHostBuilder>? extensionPoint = null) =>
            HostService(args, extensionPoint, CreateRestHostBuilder);

        #endregion HostRest

        #region OnUnhandledException

        /// <summary>
        /// Called when [unhandled exception].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            string terminating = args.IsTerminating ? "terminating" : "";
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Unhandled [{terminating}]: {e.FormatLazy()}");
            Console.ResetColor();
        }

        #endregion // OnUnhandledException

        #region HostService

        /// <summary>
        /// Starts hosting.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="extensionPoint">The extension point.</param>
        /// <param name="strategy">The strategy.</param>
        public void HostService(
                    string[] args,
                    Action<IHostBuilder>? extensionPoint,
                    Func<IHostBuilder, string[], IHostBuilder> strategy)
        {

            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            try
            {
                extensionPoint?.Invoke(hostBuilder);

                strategy(hostBuilder, args).Build().Run();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Application start-up failed: {ex.FormatLazy()}");
            }
        }

        #endregion HostService

        #region CreateRestHostBuilder

        /// <summary>
        /// Creates the rest host builder.
        /// </summary>
        /// <param name="hostBuilder">The host builder.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public IHostBuilder CreateRestHostBuilder(IHostBuilder hostBuilder, string[] args) =>

                hostBuilder.UseRestDefaultsWeknow<TSetup>(args);

        #endregion CreateRestHostBuilder

        #region CreateGrpcHostBuilder

        /// <summary>
        /// Creates the GRPC host builder.
        /// </summary>
        /// <param name="hostBuilder">The host builder.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public IHostBuilder CreateGrpcHostBuilder(IHostBuilder hostBuilder, string[] args) =>
            hostBuilder.UseGrpcDefaultsWeknow<TSetup>(args);

        #endregion CreateGrpcHostBuilder
    }
}
