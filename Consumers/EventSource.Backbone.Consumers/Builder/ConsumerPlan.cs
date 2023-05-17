using System.Collections.Immutable;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

using Polly;

using EventSource.Backbone.Building;
using EventSource.Backbone.Private;

namespace EventSource.Backbone
{

    /// <summary>
    /// Hold builder definitions.
    /// Define the consumer execution pipeline.
    /// </summary>
    [DebuggerDisplay("{Environment}:{Partition}:{Shard}, Consumer: [{ConsumerGroup}, {ConsumerName}]")]
    public class ConsumerPlan : IConsumerPlan, IConsumerPlanBuilder
    {
        public static readonly ConsumerPlan Empty = new ConsumerPlan();
        private static readonly Task<ImmutableArray<IConsumerStorageStrategyWithFilter>> EMPTY_STORAGE_STRATEGIES = Task.FromResult(ImmutableArray<IConsumerStorageStrategyWithFilter>.Empty);

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        private ConsumerPlan()
        {
            ResiliencePolicy = Policy.Handle<Exception>()
                          .WaitAndRetryAsync(3,
                                (retryCount) => TimeSpan.FromSeconds(Math.Pow(2, retryCount)),
                                 ((ex, duration, retryCount, ctx) => Logger?.LogWarning(ex, "Retry {count}", retryCount)));

        }


        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="copyFrom">The copy from.</param>
        /// <param name="channelFactory">The channel.</param>
        /// <param name="channel">The channel.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="partition">The partition.</param>
        /// <param name="shard">The shard.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The options.</param>
        /// <param name="segmentationStrategies">The segmentation strategies.</param>
        /// <param name="interceptors">The interceptors.</param>
        /// <param name="routes">The routes.</param>
        /// <param name="cancellation">The cancellation token.</param>
        /// <param name="consumerGroup">Consumer Group allow a group of clients to cooperate
        /// consuming a different portion of the same stream of messages</param>
        /// <param name="consumerName">Optional Name of the consumer.
        /// Can use for observability.</param>
        /// <param name="resiliencePolicy">The resilience policy.</param>
        /// <param name="storageStrategyFactories">The storage strategy.</param>
        private ConsumerPlan(
            ConsumerPlan copyFrom,
            Func<ILogger, IConsumerChannelProvider>? channelFactory = null,
            IConsumerChannelProvider? channel = null,
            string? environment = null,
            string? partition = null,
            string? shard = null,
            ILogger? logger = null,
            ConsumerOptions? options = null,
            IImmutableList<IConsumerAsyncSegmentationStrategy>? segmentationStrategies = null,
            IImmutableList<IConsumerAsyncInterceptor>? interceptors = null,
            IImmutableList<IConsumerHooksBuilder>? routes = null,
            CancellationToken? cancellation = null,
            string? consumerGroup = null,
            string? consumerName = null,
            AsyncPolicy? resiliencePolicy = null,
            Func<ILogger, Task<IConsumerStorageStrategyWithFilter>>? storageStrategyFactories = null)
        {
            ChannelFactory = channelFactory ?? copyFrom.ChannelFactory;
            _channel = channel ?? copyFrom._channel;
            Environment = environment ?? copyFrom.Environment;
            Partition = partition ?? copyFrom.Partition;
            Shard = shard ?? copyFrom.Shard;
            Logger = logger ?? copyFrom.Logger;
            Options = options ?? copyFrom.Options;
            SegmentationStrategies = segmentationStrategies ?? copyFrom.SegmentationStrategies;
            Interceptors = interceptors ?? copyFrom.Interceptors;
            Routes = routes ?? copyFrom.Routes;
            ConsumerGroup = consumerGroup ?? copyFrom.ConsumerGroup;
            ConsumerName = consumerName ?? copyFrom.ConsumerName;
            ResiliencePolicy = resiliencePolicy ?? copyFrom.ResiliencePolicy;
            StorageStrategyFactories = storageStrategyFactories == null
                  ? copyFrom.StorageStrategyFactories
                  : copyFrom.StorageStrategyFactories.Add(storageStrategyFactories);
            StorageStrategiesAsync = copyFrom.StorageStrategiesAsync;
            if (cancellation == null)
                Cancellation = copyFrom.Cancellation;
            else
            {
                var combine = CancellationTokenSource.CreateLinkedTokenSource(cancellation ?? CancellationToken.None, copyFrom.Cancellation);
                Cancellation = combine.Token;
            }
        }

        #endregion // Ctor

        #region Channel

        /// <summary>
        /// Gets the communication channel provider.
        /// </summary>
        public Func<ILogger, IConsumerChannelProvider> ChannelFactory { get; } = (logger) =>
        {
            string log = "Event Source Consumer channel not set";
            logger.LogError(log);
            throw new ArgumentNullException(log);
        };

        private readonly IConsumerChannelProvider? _channel;
        /// <summary>
        /// Gets the communication channel provider.
        /// </summary>
        IConsumerChannelProvider IConsumerPlan.Channel
        {
            get
            {
                if (_channel == null)
                    throw new ArgumentNullException("Event Source Consumer channel not set");
                return _channel;
            }
        }

        #endregion // Channel

        #region StorageStrategyFactories

        /// <summary>
        /// Gets the storage strategy.
        /// </summary>
        public ImmutableArray<Func<ILogger, Task<IConsumerStorageStrategyWithFilter>>> StorageStrategyFactories { get; } = ImmutableArray<Func<ILogger, Task<IConsumerStorageStrategyWithFilter>>>.Empty;

        #endregion // StorageStrategyFactories

        #region StorageStrategiesAsync

        /// <summary>
        /// Gets the storage strategies.
        /// </summary>
        public Task<ImmutableArray<IConsumerStorageStrategyWithFilter>> StorageStrategiesAsync { get; init; } = EMPTY_STORAGE_STRATEGIES;

        #endregion // StorageStrategiesAsync

        #region Logger

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public ILogger Logger { get; } = EventSourceFallbakLogger.Default;

        #endregion // Channel

        #region Environment

        /// <summary>
        /// Environment (part of the stream key)
        /// </summary>
        public Env Environment { get; } = string.Empty;

        #endregion // Environment

        #region Partition

        /// <summary>
        /// Partition key represent logical group of 
        /// event source shards.
        /// For example assuming each ORDERING flow can have its 
        /// own messaging sequence, yet can live concurrency with 
        /// other ORDER's sequences.
        /// The partition will let consumer the option to be notify and
        /// consume multiple shards from single consumer.
        /// This way the consumer can handle all orders in
        /// central place without affecting sequence of specific order 
        /// flow or limiting the throughput.
        /// </summary>
        public string Partition { get; } = string.Empty;

        #endregion // Partition

        #region Shard

        /// <summary>
        /// Shard key represent physical sequence.
        /// Use same shard when order is matter.
        /// For example: assuming each ORDERING flow can have its 
        /// own messaging sequence, in this case you can split each 
        /// ORDER into different shard and gain performance bust..
        /// </summary>
        public string Shard { get; } = string.Empty;

        #endregion // Shard

        #region Options

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public ConsumerOptions Options { get; } = new ConsumerOptions();

        #endregion // Options

        #region SegmentationStrategies

        /// <summary>
        /// Segmentation responsible of splitting an instance into segments.
        /// Segments is how the Consumer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        public IImmutableList<IConsumerAsyncSegmentationStrategy> SegmentationStrategies { get; } =
                    ImmutableList<IConsumerAsyncSegmentationStrategy>.Empty;

        #endregion // SegmentationStrategies

        #region Interceptors

        /// <summary>
        /// Consumer interceptors (Timing: after serialization).
        /// </summary>
        /// <value>
        /// The interceptors.
        /// </value>
        public IImmutableList<IConsumerAsyncInterceptor> Interceptors { get; } =
                    ImmutableList<IConsumerAsyncInterceptor>.Empty;

        #endregion // Interceptors

        #region Cancellation

        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        public CancellationToken Cancellation { get; } = default;

        #endregion // Cancellation

        #region Routes

        /// <summary>
        /// Routes are sub-pipelines are results of merge operation
        /// which can split same payload into multiple partitions or shards.
        /// </summary>
        private readonly IImmutableList<IConsumerHooksBuilder> Routes =
                ImmutableList<IConsumerHooksBuilder>.Empty;

        #endregion // Routes

        #region ConsumerGroup

        /// <summary>
        /// Gets the consumer group.
        /// Consumer Group allow a group of clients to cooperate
        /// consuming a different portion of the same stream of messages
        /// </summary>
        public string ConsumerGroup { get; } = string.Empty;

        #endregion // ConsumerGroup

        #region ConsumerName

        /// <summary>
        /// Optional Name of the consumer.
        /// Can use for observability.
        /// </summary>
        public string ConsumerName { get; } = Guid.NewGuid().ToString("N");

        #endregion // ConsumerName

        #region ResiliencePolicy

        /// <summary>
        /// Gets or sets the invocation resilience policy.
        /// </summary>
        public AsyncPolicy ResiliencePolicy { get; }

        #endregion // ResiliencePolicy

        //------------------------------------------

        #region GetParameterAsync

        /// <summary>
        /// Get parameter value from the announcement.
        /// </summary>
        /// <typeparam name="TParam">The type of the parameter.</typeparam>
        /// <param name="arg">The argument.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        async ValueTask<TParam> IConsumerPlan.GetParameterAsync<TParam>(
                                Announcement arg,
                                string argumentName)
        {
            foreach (var strategy in SegmentationStrategies)
            {
                var (isValid, value) = await strategy.TryUnclassifyAsync<TParam>(arg.Metadata, arg.Segments, argumentName, Options);
                if (isValid)
                    return value;
            }
            throw new NotSupportedException($"Consumer didn't find arg:[{argumentName}]");
        }

        #endregion // GetParameterAsync

        #region WithChannel

        /// <summary>
        /// Attach the channel.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <returns></returns>
        internal ConsumerPlan WithChannelFactory(Func<ILogger, IConsumerChannelProvider> channel)
        {
            return new ConsumerPlan(this, channelFactory: channel);
        }

        #endregion // WithChannel

        #region WithStorageStrategy

        /// <summary>
        /// Attach the Storage Strategy.
        /// </summary>
        /// <param name="storageStrategy">The storage strategy.</param>
        /// <returns></returns>
        internal ConsumerPlan WithStorageStrategy(Func<ILogger, Task<IConsumerStorageStrategyWithFilter>> storageStrategy)
        {
            return new ConsumerPlan(this, storageStrategyFactories: storageStrategy);
        }

        #endregion // WithStorageStrategy

        #region WithLogger

        /// <summary>
        /// Attach the LOGGER.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        internal ConsumerPlan WithLogger(ILogger logger)
        {
            return new ConsumerPlan(this, logger: logger);
        }

        #endregion // WithChannel

        #region WithCancellation

        /// <summary>
        /// Attach the cancellation.
        /// </summary>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        internal ConsumerPlan WithCancellation(
                                        CancellationToken cancellation)
        {

            return new ConsumerPlan(this, cancellation: cancellation);
        }

        #endregion // WithCancellation

        #region WithOptions

        /// <summary>
        /// Attach the options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        internal ConsumerPlan WithOptions(ConsumerOptions options)
        {
            return new ConsumerPlan(this, options: options);
        }

        #endregion // WithOptions

        #region WithEnvironment

        /// <summary>
        /// Attach the environment.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <returns></returns>
        internal ConsumerPlan WithEnvironment(Env environment)
        {
            return new ConsumerPlan(this, environment: environment);
        }

        #endregion // WithEnvironment

        #region ChangeEnvironment

        /// <summary>
        /// Attach the environment.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <returns></returns>
        IConsumerPlan IConsumerPlan.ChangeEnvironment(Env? environment)
        {
            if (environment == null) return this;

            return new ConsumerPlan(this, environment: environment);
        }

        #endregion // ChangeEnvironment

        #region ChangePartition

        /// <summary>
        /// Attach the environment.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <returns></returns>
        IConsumerPlan IConsumerPlan.ChangePartition(Env? partition)
        {
            if (partition == null) return this;

            return new ConsumerPlan(this, partition: partition);
        }

        #endregion // ChangePartition

        #region WithPartition

        /// <summary>
        /// Attach the partition.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <returns></returns>
        internal ConsumerPlan WithPartition(string partition)
        {
            return new ConsumerPlan(this, partition: partition);
        }

        #endregion // WithPartition

        #region WithShard

        /// <summary>
        /// Attach the shard.
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <returns></returns>
        IConsumerPlan IConsumerPlanBase.WithShard(string shard) => WithShard(shard);
        /// <summary>
        /// Attach the shard.
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <returns></returns>
        internal ConsumerPlan WithShard(string shard)
        {
            return new ConsumerPlan(this, shard: shard);
        }

        #endregion // WithShard

        #region WithResiliencePolicy

        /// <summary>
        /// Set resilience policy
        /// </summary>
        /// <param name="policy">The policy.</param>
        /// <returns></returns>
        internal ConsumerPlan WithResiliencePolicy(AsyncPolicy policy)
        {
            return new ConsumerPlan(this, resiliencePolicy: policy);
        }

        #endregion // WithResiliencePolicy

        #region AddRoute

        /// <summary>
        /// Adds the route.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns></returns>
        internal ConsumerPlan AddRoute(
                                                IConsumerHooksBuilder route)
        {
            return new ConsumerPlan(this, routes: Routes.Add(route));
        }

        #endregion // AddRoute

        #region AddSegmentation

        /// <summary>
        /// Adds the segmentation.
        /// </summary>
        /// <param name="segmentation">The segmentation.</param>
        /// <returns></returns>
        internal ConsumerPlan AddSegmentation(
                                                IConsumerAsyncSegmentationStrategy segmentation)
        {
            return new ConsumerPlan(this,
                            segmentationStrategies: SegmentationStrategies.Add(segmentation));
        }

        #endregion // AddSegmentation

        #region AddInterceptor

        /// <summary>
        /// Adds the interceptor.
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns></returns>
        internal ConsumerPlan AddInterceptor(
                                  IConsumerAsyncInterceptor interceptor)
        {
            return new ConsumerPlan(this,
                            interceptors: Interceptors.Add(interceptor));
        }

        #endregion // AddInterceptor

        #region ConsumerGroup

        /// <summary>
        /// Set Consumer Group which allow a group of clients to cooperate
        /// consuming a different portion of the same stream of messages
        /// </summary>
        /// <param name="consumerGroup">
        /// Consumer Group allow a group of clients to cooperate
        /// consuming a different portion of the same stream of messages
        /// </param>
        /// <param name="consumerName">
        /// Optional Name of the consumer.
        /// Can use for observability.
        /// </param>
        /// <returns></returns>
        internal ConsumerPlan WithConsumerGroup(
            string consumerGroup,
            string? consumerName = null) => new ConsumerPlan(this,
                                                consumerGroup: consumerGroup,
                                                consumerName: consumerName);

        #endregion // ConsumerGroup

        #region WithConsumerName

        /// <summary>
        /// Withes the name of the consumer.
        /// </summary>
        /// <param name="consumerName">Name of the consumer.</param>
        /// <returns></returns>
        internal ConsumerPlan WithConsumerName(
            string? consumerName = null) => new ConsumerPlan(this,
                                                consumerName: consumerName);

        #endregion // WithConsumerName

        // --------------------------------------------------------

        #region Build

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        IConsumerPlan IConsumerPlanBuilder.Build()
        {
            var channel = ChannelFactory(Logger);
            var plan = new ConsumerPlan(this, channel: channel)
            {
                StorageStrategiesAsync = LocalAsync()
            };

            return plan;

            async Task<ImmutableArray<IConsumerStorageStrategyWithFilter>> LocalAsync()
            {
                var strategies = await Task.WhenAll(StorageStrategyFactories.Select(m => m(Logger)));
                return ImmutableArray.CreateRange(strategies);
            }
        }

        #endregion // Build
    }
}
