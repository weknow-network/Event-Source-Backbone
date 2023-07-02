using EventSourcing.Backbone.Building;

using Microsoft.Extensions.Logging;

using static System.String;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public class ProducerBuilder :
        IProducerBuilder,
        IProducerHooksBuilder,
        IProducerIocStoreStrategyBuilder
    {

        /// <summary>
        /// Event Source producer builder.
        /// </summary>
        public static readonly IProducerBuilder Empty = new ProducerBuilder();

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        private ProducerBuilder()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="plan">The plan.</param>
        internal ProducerBuilder(ProducerPlan plan)
        {
            Plan = plan;
        }

        #endregion // Ctor

        #region Plan

        /// <summary>
        /// Gets the producer's plan.
        /// </summary>
        public ProducerPlan Plan { get; } = ProducerPlan.Empty;

        #endregion // Plan

        #region Merge

        /// <summary>
        /// Merges multiple channels of same contract into single
        /// producer for broadcasting messages via all channels.
        /// </summary>
        /// <param name="first">The first channel.</param>
        /// <param name="second">The second channel.</param>
        /// <param name="others">The others channels.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IProducerHooksBuilder IProducerBuilder.Merge(
                            IProducerHooksBuilder first,
                            IProducerHooksBuilder second,
                            params IProducerHooksBuilder[] others)
        {
            #region Validation

            if (Plan == null)
                throw new EventSourcingException($"{nameof(Plan)} is null");

            #endregion // Validation

            var prms = Plan.AddForward(first);
            prms = prms.AddForward(second);
            foreach (var p in others)
            {
                prms = prms.AddForward(p);
            }
            return new ProducerBuilder(prms);
        }

        #endregion // Merge

        #region UseChannel

        /// <summary>
        /// Choose the communication channel provider.
        /// </summary>
        /// <param name="channel">The channel provider.</param>
        /// <returns></returns>
        IProducerStoreStrategyBuilder IProducerBuilder.UseChannel(
                                    Func<ILogger, IProducerChannelProvider> channel)
        {
            var prms = Plan.UseChannel(channel);
            return new ProducerBuilder(prms);
        }

        /// <summary>
        /// Choose the communication channel provider.
        /// </summary>
        /// <param name="channel">The channel provider.</param>
        /// <param name="serviceProvider">Dependency injection provider.</param>
        /// <returns></returns>
        IProducerIocStoreStrategyBuilder IProducerBuilder.UseChannel(
                                    IServiceProvider serviceProvider,
                                    Func<ILogger, IProducerChannelProvider> channel)
        {
            var prms = Plan.UseChannel(channel, serviceProvider);
            return new ProducerBuilder(prms);
        }

        #endregion // UseChannel

        #region ServiceProvider

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        IServiceProvider IProducerIocStoreStrategyBuilder.ServiceProvider =>
                            Plan?.ServiceProvider ?? throw new EventSourcingException("ServiceProvider is null");

        #endregion // ServiceProvider

        #region AddStorageStrategy

        /// <summary>
        /// Adds the storage strategy (Segment / Interceptions).
        /// Will use default storage (REDIS Hash) when empty.
        /// When adding more than one it will to all, act as a fall-back (first win, can use for caching).
        /// It important the consumer's storage will be in sync with this setting.
        /// </summary>
        /// <param name="storageStrategy">Storage strategy provider.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="filter">The filter of which keys in the bucket will be store into this storage.</param>
        /// <returns></returns>
        IProducerStoreStrategyBuilder IProducerStoreStrategyBuilder.AddStorageStrategy(
                                            Func<ILogger, ValueTask<IProducerStorageStrategy>> storageStrategy,
                                            EventBucketCategories targetType,
                                            Predicate<string>? filter)
        {
            var prms = Plan.WithStorageStrategy(Local);
            return new ProducerBuilder(prms);

            async Task<IProducerStorageStrategyWithFilter> Local(ILogger logger)
            {
                var strategy = await storageStrategy(logger);
                var decorated = new FilteredStorageStrategy(strategy, filter, targetType);
                return decorated;
            }

        }

        #endregion // AddStorageStrategy

        #region Environment

        /// <summary>
        /// Origin environment of the message
        /// </summary>
        /// <returns></returns>
        IProducerUriBuilder IProducerBuilderEnvironment<IProducerUriBuilder>.Environment(Env? environment)
        {
            if (environment == null)
                return this;

            var prms = Plan.WithEnvironment(environment);
            return new ProducerBuilder(prms);
        }


        /// <summary>
        /// Origin environment of the message
        /// </summary>
        /// <returns></returns>
        IProducerSpecializeBuilder IProducerBuilderEnvironment<IProducerSpecializeBuilder>.Environment(Env? environment)
        {
            if (environment == null)
                return this;

            var prms = Plan.WithEnvironment(environment);
            return new ProducerBuilder(prms);
        }

        #endregion // Environment

        #region EnvironmentFromVariable

        /// <summary>
        /// Fetch the origin environment of the message from an environment variable.
        /// </summary>
        /// <param name="environmentVariableKey">The environment variable key.</param>
        /// <returns></returns>
        /// <exception cref="EventSourcing.Backbone.EventSourcingException">EnvironmentFromVariable failed, [{environmentVariableKey}] not found!</exception>
        IProducerUriBuilder IProducerBuilderEnvironment<IProducerUriBuilder>.EnvironmentFromVariable(string environmentVariableKey)
        {
            string environment = Environment.GetEnvironmentVariable(environmentVariableKey) ?? throw new EventSourcingException($"EnvironmentFromVariable failed, [{environmentVariableKey}] not found!");
            var prms = Plan.WithEnvironment(environment);
            return new ProducerBuilder(prms);
        }


        /// <summary>
        /// Fetch the origin environment of the message from an environment variable.
        /// </summary>
        /// <param name="environmentVariableKey">The environment variable key.</param>
        /// <returns></returns>
        /// <exception cref="EventSourcing.Backbone.EventSourcingException">EnvironmentFromVariable failed, [{environmentVariableKey}] not found!</exception>
        IProducerSpecializeBuilder IProducerBuilderEnvironment<IProducerSpecializeBuilder>.EnvironmentFromVariable(string environmentVariableKey)
        {
            string environment = Environment.GetEnvironmentVariable(environmentVariableKey) ?? throw new EventSourcingException($"EnvironmentFromVariable failed, [{environmentVariableKey}] not found!");
            var prms = Plan.WithEnvironment(environment);
            return new ProducerBuilder(prms);
        }

        #endregion // EnvironmentFromVariable

        #region Key

        /// <summary>
        /// The stream's key
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        IProducerHooksBuilder IProducerUriBuilder.Uri(string uri)
        {
            var prms = Plan.WithKey(uri);
            return new ProducerBuilder(prms);
        }

        #endregion // Key

        #region WithOptions

        /// <summary>
        /// Apply configuration.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IProducerEnvironmentBuilder IProducerOptionsBuilder.WithOptions(EventSourceOptions options)
        {
            var prms = Plan.WithOptions(options);
            return new ProducerBuilder(prms);
        }

        #endregion // WithOptions

        #region UseSegmentation

        /// <summary>
        /// Register segmentation strategy,
        /// Segmentation responsible of splitting an instance into segments.
        /// Segments is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="segmentationStrategy">A strategy of segmentation.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        IProducerHooksBuilder IProducerHooksBuilder.UseSegmentation(
                            IProducerAsyncSegmentationStrategy segmentationStrategy)
        {
            var prms = Plan.AddSegmentation(segmentationStrategy);
            return new ProducerBuilder(prms);
        }

        /// <summary>
        /// Register segmentation strategy,
        /// Segmentation responsible of splitting an instance into segments.
        /// Segments is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="segmentationStrategy">A strategy of segmentation.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        IProducerHooksBuilder IProducerHooksBuilder.UseSegmentation(IProducerSegmentationStrategy segmentationStrategy)
        {
            var asyncImp = new ProducerSegmentationStrategyBridge(segmentationStrategy);
            var prms = Plan.AddSegmentation(asyncImp);
            return new ProducerBuilder(prms);
        }

        #endregion // UseSegmentation

        #region AddInterceptor

        /// <summary>
        /// Adds Producer interceptor (stage = after serialization).
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IProducerHooksBuilder IProducerHooksBuilder.AddInterceptor(
                                                IProducerInterceptor interceptor)
        {
            var bridge = new ProducerInterceptorBridge(interceptor);
            var prms = Plan.AddInterceptor(bridge);
            return new ProducerBuilder(prms);
        }

        /// <summary>
        /// Adds Producer interceptor (Timing: after serialization).
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IProducerHooksBuilder IProducerHooksBuilder.AddInterceptor(
            IProducerAsyncInterceptor interceptor)
        {
            var prms = Plan.AddInterceptor(interceptor);
            return new ProducerBuilder(prms);
        }

        #endregion // AddInterceptor

        #region WithLogger

        /// <summary>
        /// Attach logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        private ProducerBuilder WithLogger(ILogger logger)
        {
            var prms = Plan.WithLogger(logger);
            return new ProducerBuilder(prms);
        }

        /// <summary>
        /// Attach logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        IProducerSpecializeBuilder IProducerLoggerBuilder<IProducerSpecializeBuilder>.WithLogger(ILogger logger)
        {
            return WithLogger(logger);
        }

        /// <summary>
        /// Attach logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        IProducerUriBuilder IProducerLoggerBuilder<IProducerUriBuilder>.WithLogger(ILogger logger)
        {
            return WithLogger(logger);
        }


        #endregion // WithLogger

        #region Build

        /// <summary>
        /// <![CDATA[ Ceate Producer proxy for specific events sequence.
        /// Event sequence define by an interface which declare the
        /// operations which in time will be serialized into event's
        /// messages.
        /// This interface can be use as a proxy in the producer side,
        /// and as adapter on the consumer side.
        /// All method of the interface should represent one-way communication pattern
        /// and return Task or ValueTask (not Task<T> or ValueTask<T>).
        /// Nothing but method allowed on this interface]]>
        /// </summary>
        /// <typeparam name="T">The contract of the proxy / adapter</typeparam>
        /// <param name="factory">The factory.</param>
        /// <returns></returns>
        T IProducerSpecializeBuilder.Build<T>(Func<IProducerPlan, T> factory)
        {
            var planBuilder = Plan;
            if (planBuilder.SegmentationStrategies.Count == 0)
                planBuilder = planBuilder.AddSegmentation(new ProducerDefaultSegmentationStrategy());
            var plan = ((IProducerPlanBuilder)planBuilder).Build(); // attach he channel
            return factory(plan);
        }

        #endregion // Build

        #region BuildRaw

        /// <summary>
        /// <![CDATA[ Ceate Producer proxy for raw events sequence.
        /// Useful for data migration at the raw data level.
        /// This producer can be attached to a subscriber and route it data elesewhere.
        /// For example, alternative cloud provider, qa environement, etc.]]>
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        IRawProducer IProducerRawBuilder.BuildRaw(RawProducerOptions? options)
        {
            var planBuilder = Plan;
            IProducerPlan plan = ((IProducerPlanBuilder)planBuilder).Build(); // attach he channel
            return new RawProducer(plan, options);
        }

        #endregion // BuildRaw

        #region Override

        /// <summary>
        /// Enable dynamic transformation of the stream id before sending.
        /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IProducerOverrideBuilder<T> IProducerSpecializeBuilder.Override<T>() => new Router<T>(Plan);

        #endregion // Override

        #region Specialize

        /// <summary>
        /// Enable dynamic transformation of the stream id before sending.
        /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IProducerOverrideBuilder<T> IProducerSpecializeBuilder.Specialize<T>() => new Router<T>(Plan);

        #endregion // Specialize

        #region Router

        /// <summary>
        /// Enable dynamic transformation of the stream id before sending.
        /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private sealed class Router<T> : IProducerOverrideBuilder<T> where T : class
        {
            private readonly ProducerPlan _plan;

            #region Ctor

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="plan">The plan.</param>
            public Router(ProducerPlan plan)
            {
                _plan = plan;
            }

            #endregion // Ctor

            #region Build

            /// <summary>
            /// <![CDATA[ Ceate Producer proxy for specific events sequence.
            /// Event sequence define by an interface which declare the
            /// operations which in time will be serialized into event's
            /// messages.
            /// This interface can be use as a proxy in the producer side,
            /// and as adapter on the consumer side.
            /// All method of the interface should represent one-way communication pattern
            /// and return Task or ValueTask (not Task<T> or ValueTask<T>).
            /// Nothing but method allowed on this interface]]>
            /// </summary>
            /// <param name="factory">The factory.</param>
            /// <returns></returns>
            T IProducerOverrideBuildBuilder<T>.Build(Func<IProducerPlan, T> factory)
            {
                var planBuilder = _plan;
                if (planBuilder.SegmentationStrategies.Count == 0)
                    planBuilder = planBuilder.AddSegmentation(new ProducerDefaultSegmentationStrategy());
                var plan = ((IProducerPlanBuilder)planBuilder).Build(); // attach he channel
                return factory(plan);
            }

            #endregion // Build

            #region Strategy

            /// <summary>
            /// Dynamic override of the stream id before sending.
            /// Can use for scenario like routing between environment like dev vs. prod or AWS vs azure.
            /// </summary>
            /// <param name="routeStrategy">The routing strategy.</param>
            /// <returns></returns>
            IProducerOverrideBuildBuilder<T> IProducerOverrideBuilder<T>.Strategy(Func<IPlanRoute, (string? environment, string? uri)> routeStrategy)
            {
                var (environment, uri) = routeStrategy(_plan);
                var plan = _plan.WithEnvironment(environment ?? _plan.Environment, uri);
                return new Router<T>(plan);
            }

            #endregion // Strategy

            #region Environment

            /// <summary>
            /// Override the environment.
            /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
            /// </summary>
            /// <param name="environment">The environment.</param>
            /// <returns></returns>
            IProducerOverrideUriBuilder<T> IProducerOverrideEnvironmentBuilder<T>.Environment(Env environment)
            {
                var plan = _plan.WithEnvironment(environment ?? _plan.Environment);
                return new Router<T>(plan);
            }

            #endregion // Environment

            #region Uri

            /// <summary>
            /// Override the URI.
            /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
            /// </summary>
            /// <param name="uri">The URI.</param>
            /// <param name="type">The type.</param>
            /// <returns></returns>
            IProducerOverrideBuildBuilder<T> IProducerOverrideUriBuilder<T>.Uri(string uri, RouteAssignmentType type)
            {
                var plan = _plan.WithKey(uri, type: type);
                return new Router<T>(plan);
            }

            #endregion // Uri
        }

        #endregion // Router

        #region RawProducer

        /// <summary>
        /// Raw producer (useful for cluster migration)
        /// </summary>
        /// <seealso cref="EventSourcing.Backbone.IRawProducer" />
        private sealed class RawProducer : IRawProducer
        {
            private readonly IProducerPlan _plan;
            private readonly RawProducerOptions? _options;

            #region Ctor

            /// <summary>
            /// Initializes a new instance of the <see cref="RawProducer" /> class.
            /// </summary>
            /// <param name="plan">The plan.</param>
            /// <param name="options">The options.</param>
            public RawProducer(IProducerPlan plan, RawProducerOptions? options)
            {
                _plan = plan;
                _options = options;
            }

            #endregion // Ctor

            /// <summary>
            /// <![CDATA[Producer proxy for raw events sequence.
            /// Useful for data migration at the raw data level.]]>
            /// </summary>
            /// <param name="data"></param>
            /// <returns></returns>
            public async ValueTask Produce(Announcement data)
            {
                var strategies = await _plan.StorageStrategiesAsync;
                Metadata metadata = data.Metadata;
                Metadata meta = metadata;
                if (!(_options?.KeepOriginalMeta ?? false))
                {
                    meta = meta with
                    {
                        MessageId = Guid.NewGuid().ToString("N"),
                        Environment = IsNullOrEmpty(_plan.Environment) ? metadata.Environment : _plan.Environment,
                        Uri = IsNullOrEmpty(_plan.Uri) ? metadata.Uri : _plan.Uri,
                        Origin = MessageOrigin.Copy,
                        Linked = metadata,
                    };
                }
                data = data with { Metadata = meta };
                await _plan.Channel.SendAsync(_plan, data, strategies);
            }
        }

        #endregion // RawProducer
    }
}
