using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Weknow.EventSource.Backbone;
using Weknow.EventSource.Backbone.Building;

// TODO: Register the service at the Program.cs file services.AddHostedService<...>



namespace Weknow.EventSource.Backbone.WebEventTest.Jobs
{
    /// <summary>
    /// MicroDemo Event Source Listener
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Hosting.HostedServiceBase" />
    /// <seealso cref="System.IDisposable" />
    public sealed class MigrationJob : HostedServiceBase, IDisposable
    {
        private readonly IConsumerSubscribeBuilder _builder;
        private readonly IEventsMigration _forwarder;
        private readonly HttpClient _client;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="consumerBuilder">The builder.</param>
        /// <param name="forwarder">The forwarder.</param>
        public MigrationJob(
            ILogger<MigrationJob> logger,
            IConsumerReadyBuilder consumerBuilder,
            IEventsMigration forwarder,
            IHttpClientFactory clientFactory)
            : base(logger)
        {
            _builder = consumerBuilder.WithLogger(logger);
            _forwarder = forwarder;
            _client = clientFactory.CreateClient("migration");
        }

        #endregion Ctor

        #region OnStartAsync

        /// <summary>
        /// Execution starting point.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected override async Task OnStartAsync(CancellationToken cancellationToken)
        {
            SubscriptionBridge subscription = new(_forwarder, _client);
            _builder.Group("Demo-Migration-GROUP")
                    .Subscribe(subscription);

            var tcs = new TaskCompletionSource();
            cancellationToken.Register(() => tcs.TrySetResult());
            await tcs.Task;
        }

        #endregion OnStartAsync

        #region Dispose

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion Dispose

        private class SubscriptionBridge : ISubscriptionBridge
        {
            private readonly IEventsMigration _fw;
            private readonly HttpClient _client;

            public SubscriptionBridge(IEventsMigration fw, HttpClient client)
            {
                _fw = fw;
                _client = client;
            }

            public async Task<bool> BridgeAsync(Announcement announcement, IConsumerBridge consumerBridge)
            {
                Metadata meta = announcement.Metadata;
                if ((meta.Origin & MessageOrigin.Original) == MessageOrigin.None)
                    return false; // avoid infinite loop
                await _fw.ForwardEventAsync(announcement);
                return true;

            }
        }
    }
}
