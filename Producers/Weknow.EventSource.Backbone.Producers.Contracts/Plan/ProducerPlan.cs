using Microsoft.Extensions.Logging;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.Private;

namespace Weknow.EventSource.Backbone
{

    /// <summary>
    /// Hold builder definitions.
    /// Define the consumer execution pipeline.
    /// </summary>
    public class ProducerPlan : IProducerPlan, IProducerPlanBuilder
    {
        public static readonly ProducerPlan Empty = new ProducerPlan();
        private static readonly Task<ImmutableArray<IProducerStorageStrategyWithFilter>> EMPTY_STORAGE_STRATEGY = Task.FromResult(ImmutableArray<IProducerStorageStrategyWithFilter>.Empty);

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        private ProducerPlan()
        {
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
        /// <param name="forwards">Result of merging multiple channels.</param>
        /// <param name="forwardPlans">The forward channels.</param>
        /// <param name="storageStrategyFactories">The storage strategy.</param>
        private ProducerPlan(
            ProducerPlan copyFrom,
            Func<ILogger, IProducerChannelProvider>? channelFactory = null,
            IProducerChannelProvider? channel = null,
            string? environment = null,
            string? partition = null,
            string? shard = null,
            ILogger? logger = null,
            EventSourceOptions? options = null,
            IImmutableList<IProducerAsyncSegmentationStrategy>? segmentationStrategies = null,
            IImmutableList<IProducerAsyncInterceptor>? interceptors = null,
            IImmutableList<IProducerHooksBuilder>? routes = null,
            IImmutableList<IProducerHooksBuilder>? forwards = null,
            IImmutableList<IProducerPlan>? forwardPlans = null,
            Func<ILogger, Task<IProducerStorageStrategyWithFilter>>? storageStrategyFactories = null)
        {
            ChannelFactory = channelFactory ?? copyFrom.ChannelFactory;
            _channel = channel ?? copyFrom._channel;
            Environment = environment ?? copyFrom.Environment;
            Partition = partition ?? copyFrom.Partition;
            Shard = shard ?? copyFrom.Shard;
            Options = options ?? copyFrom.Options;
            SegmentationStrategies = segmentationStrategies ?? copyFrom.SegmentationStrategies;
            Interceptors = interceptors ?? copyFrom.Interceptors;
            Routes = routes ?? copyFrom.Routes;
            Forwards = forwards ?? copyFrom.Forwards;
            _forwardPlans = forwardPlans ?? copyFrom._forwardPlans;
            Logger = logger ?? copyFrom.Logger;
            StorageStrategyFactories = storageStrategyFactories == null
                ? copyFrom.StorageStrategyFactories
                : copyFrom.StorageStrategyFactories.Add(storageStrategyFactories);
        }

        #endregion // Ctor

        #region Channel

        /// <summary>
        /// Gets the communication channel provider.
        /// </summary>
        public Func<ILogger, IProducerChannelProvider> ChannelFactory { get; } = (logger) =>
        {
            string log = "Event Source Producer channel not set";
            logger.LogError(log);
            throw new ArgumentNullException(log);
        };

        private readonly IProducerChannelProvider? _channel;
        /// <summary>
        /// Gets the communication channel provider.
        /// </summary>
        IProducerChannelProvider IProducerPlan.Channel
        {
            get
            {
                if (_channel == null)
                    throw new ArgumentNullException("Event Source Producer channel not set");
                return _channel;
            }
        }

        #endregion // Channel

        #region StorageStrategyFactories

        /// <summary>
        /// Gets the storage strategy.
        /// By design the stream should hold minimal information while the main payload 
        /// is segmented and can stored outside of the stream.
        /// This pattern will help us to split data for different reasons, for example GDPR PII (personally identifiable information).
        /// </summary>
        public ImmutableArray<Func<ILogger, Task<IProducerStorageStrategyWithFilter>>> StorageStrategyFactories { get; } =
                                            ImmutableArray<Func<ILogger, Task<IProducerStorageStrategyWithFilter>>>.Empty;


        #endregion // StorageStrategyFactories
        /// <summary>
        /// Gets the storage strategy.
        /// By design the stream should hold minimal information while the main payload 
        /// is segmented and can stored outside of the stream.
        /// This pattern will help us to split data for different reasons, for example GDPR PII (personally identifiable information).
        /// </summary>
        public Task<ImmutableArray<IProducerStorageStrategyWithFilter>> StorageStrategiesAsync { get; init; } = EMPTY_STORAGE_STRATEGY;

        #region Logger

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public ILogger Logger { get; } = EventSourceFallbakLogger.Default;

        #endregion // Channel

        #region Environment

        /// <summary>
        /// Origin environment of the message
        /// </summary>
        public string Environment { get; } = string.Empty;

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
        public EventSourceOptions Options { get; init; } = new EventSourceOptions();

        #endregion // Options

        #region SegmentationStrategies

        /// <summary>
        /// Segmentation responsible of splitting an instance into segments.
        /// Segments is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        public IImmutableList<IProducerAsyncSegmentationStrategy> SegmentationStrategies { get; } =
                    ImmutableList<IProducerAsyncSegmentationStrategy>.Empty;

        #endregion // SegmentationStrategies

        #region Interceptors

        /// <summary>
        /// Producer interceptors (Timing: after serialization).
        /// </summary>
        /// <value>
        /// The interceptors.
        /// </value>
        public IImmutableList<IProducerAsyncInterceptor> Interceptors { get; } =
                    ImmutableList<IProducerAsyncInterceptor>.Empty;

        #endregion // Interceptors

        #region Forwards

        /// <summary>
        /// Gets the forwards pipelines.
        /// Result of merging multiple channels.
        /// </summary>
        public IImmutableList<IProducerHooksBuilder> Forwards { get; } = ImmutableList<IProducerHooksBuilder>.Empty;

        #endregion // Forwards

        #region ForwardChannels

        IImmutableList<IProducerPlan> _forwardPlans { get; } = ImmutableList<IProducerPlan>.Empty;
        /// <summary>
        /// Gets the forwards pipelines.
        /// Result of merging multiple channels.
        /// </summary>
        IImmutableList<IProducerPlan> IProducerPlan.ForwardPlans => _forwardPlans;

        #endregion // ForwardChannels

        #region Routes

        /// <summary>
        /// Routes are sub-pipelines are results of merge operation
        /// which can split same payload into multiple partitions or shards.
        /// </summary>
        private readonly IImmutableList<IProducerHooksBuilder> Routes =
                ImmutableList<IProducerHooksBuilder>.Empty;

        #endregion // Routes

        //---------------------------------------

        #region UseChannel

        /// <summary>
        /// Assign channel.
        /// </summary>
        /// <param name="channelFactory">The channel factory.</param>
        /// <returns></returns>
        public ProducerPlan UseChannel(Func<ILogger, IProducerChannelProvider> channelFactory)
        {
            return new ProducerPlan(this, channelFactory: channelFactory);
        }

        #endregion // WithOptions

        #region WithLogger

        /// <summary>
        /// Attach logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        public ProducerPlan WithLogger(ILogger logger)
        {
            return new ProducerPlan(this, logger: logger);
        }

        #endregion // WithLogger

        #region WithStorageStrategy

        /// <summary>
        /// Attach Storage Strategy.
        /// </summary>
        /// <param name="storageStrategy">The storage strategy.</param>
        /// <returns></returns>
        public ProducerPlan WithStorageStrategy(Func<ILogger, Task<IProducerStorageStrategyWithFilter>> storageStrategy)
        {
            return new ProducerPlan(this, storageStrategyFactories: storageStrategy);
        }

        #endregion // WithStorageStrategy

        #region WithOptions

        /// <summary>
        /// Withes the options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public ProducerPlan WithOptions(EventSourceOptions options)
        {
            return new ProducerPlan(this, options: options);
        }

        #endregion // WithOptions

        #region WithEnvironment

        /// <summary>
        /// Withes the environment.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <param name="partition">The partition.</param>
        /// <param name="shard">The shard.</param>
        /// <param name="type">The type (only for partition and shard).</param>
        /// <returns></returns>
        public ProducerPlan WithEnvironment(
            string environment,
            string? partition = null,
            string? shard = null,
            RouteAssignmentType type = RouteAssignmentType.Replace)
        {
            switch (type)
            {
                case RouteAssignmentType.Prefix:
                    return new ProducerPlan(this, environment: environment, partition: $"{partition}{this.Partition}", shard: $"{shard}{this.Shard}");
                case RouteAssignmentType.Replace:
                    return new ProducerPlan(this, environment: environment, partition: partition ?? this.Partition, shard: shard ?? this.Shard);
                case RouteAssignmentType.Suffix:
                    return new ProducerPlan(this, environment: environment, partition: $"{this.Partition}{partition}", shard: $"{this.Shard}{shard}");
                default:
                    return this;
            }
        }

        #endregion // WithEnvironment

        #region WithPartition

        /// <summary>
        /// Withes the partition.
        /// </summary>
        /// <param name="partition">The partition.</param>
        /// <param name="shard">The shard.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public ProducerPlan WithPartition(string partition, string? shard = null,
            RouteAssignmentType type = RouteAssignmentType.Replace)
        {
            switch (type)
            {
                case RouteAssignmentType.Prefix:
                    return new ProducerPlan(this, partition: $"{partition}{this.Partition}", shard: $"{shard}{this.Shard}");
                case RouteAssignmentType.Replace:
                    return new ProducerPlan(this, partition: partition, shard: shard ?? this.Shard);
                case RouteAssignmentType.Suffix:
                    return new ProducerPlan(this, partition: $"{this.Partition}{partition}", shard: $"{this.Shard}{shard}");
                default:
                    return this;
            }
        }

        #endregion // WithPartition

        #region WithShard

        /// <summary>
        /// Withes the shard.
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public ProducerPlan WithShard(string shard,
            RouteAssignmentType type = RouteAssignmentType.Replace)
        {
            switch (type)
            {
                case RouteAssignmentType.Prefix:
                    return new ProducerPlan(this, shard: $"{shard}{this.Shard}");
                case RouteAssignmentType.Replace:
                    return new ProducerPlan(this, shard: shard ?? this.Shard);
                case RouteAssignmentType.Suffix:
                    return new ProducerPlan(this, shard: $"{this.Shard}{shard}");
                default:
                    return this;
            }
        }

        #endregion // WithShard

        #region AddRoute

        /// <summary>
        /// Adds the route.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns></returns>
        public ProducerPlan AddRoute(IProducerHooksBuilder route)
        {
            return new ProducerPlan(this, routes: Routes.Add(route));
        }

        #endregion // AddRoute

        #region AddSegmentation

        /// <summary>
        /// Adds the segmentation.
        /// </summary>
        /// <param name="segmentation">The segmentation.</param>
        /// <returns></returns>
        public ProducerPlan AddSegmentation(
                                                IProducerAsyncSegmentationStrategy segmentation)
        {
            return new ProducerPlan(this,
                            segmentationStrategies: SegmentationStrategies.Add(segmentation));
        }

        #endregion // AddSegmentation

        #region AddInterceptor

        /// <summary>
        /// Adds the interceptor.
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns></returns>
        public ProducerPlan AddInterceptor(
                                  IProducerAsyncInterceptor interceptor)
        {
            return new ProducerPlan(this,
                            interceptors: Interceptors.Add(interceptor));
        }

        #endregion // AddInterceptor

        #region AddForward

        public ProducerPlan AddForward(IProducerHooksBuilder forward)
        {
            return new ProducerPlan(this, forwards: Forwards.Add(forward));
        }

        #endregion // AddForward

        #region class NopChannel

        /// <summary>
        /// Not operational channel
        /// </summary>
        /// <seealso cref="Weknow.EventSource.Backbone.IProducerChannelProvider" />
        private class NopChannel : IProducerChannelProvider
        {
            public static readonly IProducerChannelProvider Empty = new NopChannel();
            public ValueTask<string> SendAsync(Announcement payload, ImmutableArray<IProducerStorageStrategyWithFilter> storageStrategy)
            {
                throw new NotSupportedException("Channel must be assign");
            }
        }

        #endregion // class NopChannel

        // ---------------------------------------

        #region Build

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        IProducerPlan IProducerPlanBuilder.Build()
        {
            if (Forwards.Count == 0)
            {
                var channel = ChannelFactory(Logger);
                var plan = new ProducerPlan(this, channel: channel)
                {
                    StorageStrategiesAsync = LocalAsync()
                };
                return plan;
            }
            var fws = Forwards.Select(fw =>
            {
                IProducerPlanBuilder p = fw.Plan;
                IProducerPlan pln = p.Build();
                return pln;
            });


            return new ProducerPlan(this, forwardPlans: ImmutableList.CreateRange(fws))
            {
                StorageStrategiesAsync = LocalAsync()
            };

            async Task<ImmutableArray<IProducerStorageStrategyWithFilter>> LocalAsync()
            {
                var strategies = await Task.WhenAll(StorageStrategyFactories.Select(m => m(Logger)));
                return ImmutableArray.CreateRange(strategies);
            }

        }

        #endregion // Build
    }
}
