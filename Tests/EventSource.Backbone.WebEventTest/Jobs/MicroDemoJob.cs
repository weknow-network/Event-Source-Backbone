using System.Text.Json;

using EventSource.Backbone.Building;

// TODO: Register the service at the Program.cs file services.AddHostedService<...>

namespace EventSource.Backbone.WebEventTest.Jobs
{
    /// <summary>
    /// MicroDemo Event Source Listener
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Hosting.HostedServiceBase" />
    /// <seealso cref="System.IDisposable" />
    public sealed class MicroDemoJob : HostedServiceBase, IDisposable
    {
        private readonly IConsumerSubscribeBuilder _builder;
        private readonly IEventFlowProducer _producer;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="consumerBuilder">The builder.</param>
        /// <param name="producer">The producer.</param>
        public MicroDemoJob(
            ILogger<MicroDemoJob> logger,
            IConsumerReadyBuilder consumerBuilder,
            IEventFlowProducer producer)
            : base(logger)
        {
            _builder = consumerBuilder.WithLogger(logger);
            _producer = producer;
        }

        #endregion Ctor

        #region OnStartAsync

        /// <summary>
        /// Execution starting point.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected override async Task OnStartAsync(CancellationToken cancellationToken)
        {
            _builder.Group("Demo-GROUP")
                    .SubscribeEventFlowConsumer(new Subscriber(_logger, _producer));

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

        private class Subscriber : IEventFlowConsumer
        {
            private readonly ILogger _logger;
            private readonly IEventFlowProducer _producer;

            public Subscriber(
                //ConsumerMetadata metadata,
                ILogger logger,
                IEventFlowProducer producer)
            {
                _logger = logger;
                _producer = producer;
            }

            async ValueTask IEventFlowConsumer.Stage1Async(Person PII, string payload)
            {
                Metadata? meta = ConsumerMetadata.Context;

                _logger.LogInformation("Consume First Stage {partition} {shard} {PII} {data}",
                    meta?.Partition, meta?.Shard, PII, payload);

                await _producer.Stage2Async(
                    JsonDocument.Parse("{\"name\":\"john\"}").RootElement,
                    JsonDocument.Parse("{\"data\":10}").RootElement);
            }

            ValueTask IEventFlowConsumer.Stage2Async(JsonElement PII, JsonElement data)
            {
                var meta = ConsumerMetadata.Context;
                _logger.LogInformation("Consume 2 Stage {partition} {shard} {PII} {data}",
                    meta?.Metadata?.Partition, meta?.Metadata?.Shard, PII, data);
                return ValueTask.CompletedTask;
            }

        }
    }

}
