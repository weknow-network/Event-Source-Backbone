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
    public sealed class MicroDemoJob : HostedServiceBase, IDisposable
    {
        private readonly IConsumerSubscribeBuilder _builder;
        private readonly ILogger<MicroDemoJob> _logger;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="microInfo">The micro information.</param>
        public MicroDemoJob(
            ILogger<MicroDemoJob> logger,
            IConsumerLoggerBuilder builder)
            : base(logger)
        {
            _builder = builder.WithLogger(logger);
            _logger = logger;
        }

        #endregion Ctor

        #region OnStartAsync

        /// <summary>
        /// Execution starting point.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected override async Task OnStartAsync(CancellationToken cancellationToken)
        {
            _builder.Subscribe<IEventFlow>(meta => new Subscriber(meta, _logger), "Demo-GROUP");

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

        private class Subscriber : IEventFlow
        {
            private readonly Metadata _metadata;
            private readonly ILogger _logger;

            public Subscriber(ConsumerMetadata metadata, ILogger logger)
            {
                _metadata = metadata.Metadata;
                _logger = logger;
            }

            ValueTask IEventFlow.Stage1Async(Person PII, string payload)
            {
                _logger.LogInformation("Consume First Stage {partition} {shard} {PII} {data}",
                    _metadata.Partition, _metadata.Shard, PII, payload);

                return ValueTaskStatic.CompletedValueTask;
            }

            ValueTask IEventFlow.Stage2Async(JsonElement PII, JsonElement data)
            {
                throw new NotImplementedException();
            }
        }
    }
}
