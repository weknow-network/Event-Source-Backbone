using System.Text.Json;

using EventSourcing.Backbone.Building;

namespace EventSourcing.Backbone.WebEventTest.Jobs
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
#pragma warning disable S1186 // Methods should not be empty
        public void Dispose()
        {
        }
#pragma warning restore S1186 // Methods should not be empty

        #endregion Dispose

        private sealed class Subscriber : IEventFlowConsumer
        {
            private readonly ILogger _logger;
            private readonly IEventFlowProducer _producer;

            public Subscriber(
                //ConsumerContext metadata,
                ILogger logger,
                IEventFlowProducer producer)
            {
                _logger = logger;
                _producer = producer;
            }

            async ValueTask IEventFlowConsumer.Stage1Async(ConsumerContext consumerMeta, Person PII, string payload)
            {
                Metadata meta = consumerMeta.Metadata;

                _logger.LogInformation("Consume First Stage {uri} {PII} {data}",
                    meta?.Uri, PII, payload);

                await _producer.Stage2Async(
                    JsonDocument.Parse("{\"name\":\"john\"}").RootElement,
                    JsonDocument.Parse("{\"data\":10}").RootElement);
            }

            ValueTask IEventFlowConsumer.Stage2Async(ConsumerContext consumerMeta, JsonElement PII, JsonElement data)
            {
                Metadata meta = consumerMeta.Metadata;
                _logger.LogInformation("Consume 2 Stage {uri} {PII} {data}",
                    meta?.Uri, PII, data);
                return ValueTask.CompletedTask;
            }

        }
    }

}
