using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.CodeGeneration;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public class ProducerBuilder :
        IProducerBuilder,
        IProducerShardBuilder,
        IProducerHooksBuilder,
        IProducerStoreStrategyBuilder
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
                throw new ArgumentNullException(nameof(Plan));

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

        #endregion // UseChannel

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
        IProducerPartitionBuilder IProducerEnvironmentBuilder.Environment(string environment)
        {
            var prms = Plan.WithEnvironment(environment);
            return new ProducerBuilder(prms);
        }

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
        /// <param name="partition">The partition key.</param>
        /// <returns></returns>
        IProducerShardBuilder IProducerPartitionBuilder.Partition(string partition)
        {
            var prms = Plan.WithPartition(partition);
            return new ProducerBuilder(prms);
        }

        #endregion // Partition

        #region Shard

        /// <summary>
        /// Shard key represent physical sequence.
        /// Use same shard when order is matter.
        /// For example: assuming each ORDERING flow can have its
        /// own messaging sequence, in this case you can split each
        /// ORDER into different shard and gain performance bust..
        /// </summary>
        /// <param name="shard">The shard key.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IProducerHooksBuilder IProducerShardBuilder.Shard(string shard)
        {
            var prms = Plan.WithShard(shard);
            return new ProducerBuilder(prms);
        }

        #endregion // Shard

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
        IProducerSpecializeBuilder IProducerLoggerBuilder.WithLogger(ILogger logger)
        {
            var prms = Plan.WithLogger(logger);
            return new ProducerBuilder(prms);
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
        /// <returns></returns>
        T IProducerSpecializeBuilder.Build<T>()
        {
            return BuildInternal<T>(Plan);
        }

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

        #region BuildInternal

        /// <summary>
        /// Builds .
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="planBuilder">The plan builder.</param>
        /// <returns></returns>
        internal static T BuildInternal<T>(ProducerPlan planBuilder)
        {
            if (planBuilder.SegmentationStrategies.Count == 0)
                planBuilder = planBuilder.AddSegmentation(new ProducerDefaultSegmentationStrategy());
            var plan = ((IProducerPlanBuilder)planBuilder).Build(); // attach he channel
            return new CodeGenerator("DynamicProxies")
                        .CreateProducerProxy<T, ProducerPipeline>(plan);
        }

        #endregion // BuildInternal

        /// <summary>
        /// Enable dynamic transformation of the stream id before sending.
        /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IProducerOverrideBuilder<T> IProducerSpecializeBuilder.Override<T>() => new Router<T>(Plan);

        #region Router

        /// <summary>
        /// Enable dynamic transformation of the stream id before sending.
        /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class Router<T> : IProducerOverrideBuilder<T> where T : class
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
            /// Builds the producer.
            /// </summary>
            /// <returns></returns>
            T IProducerOverrideBuildBuilder<T>.Build() => BuildInternal<T>(_plan);

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
            /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
            /// </summary>
            /// <param name="routeStrategy">The routing strategy.</param>
            /// <returns></returns>
            IProducerOverrideBuildBuilder<T> IProducerOverrideBuilder<T>.Strategy(Func<IProducerPlanRoute, (string? environment, string? partition, string? shard)> routeStrategy)
            {
                var (environment, partition, shard) = routeStrategy(_plan);
                var plan = _plan.WithEnvironment(environment ?? _plan.Environment, partition, shard);
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
            IProducerOverridePartitionBuilder<T> IProducerOverrideEnvironmentBuilder<T>.Environment(string environment)
            {
                var plan = _plan.WithEnvironment(environment ?? _plan.Environment);
                return new Router<T>(plan);
            }

            #endregion // Environment

            #region Partition

            /// <summary>
            /// Override the partition.
            /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
            /// </summary>
            /// <param name="partition">The partition.</param>
            /// <param name="type">The type.</param>
            /// <returns></returns>
            IProducerOverrideShardBuilder<T> IProducerOverridePartitionBuilder<T>.Partition(string partition, RouteAssignmentType type)
            {
                var plan = _plan.WithPartition(partition, type: type);
                return new Router<T>(plan);
            }

            #endregion // Partition

            #region Shard

            /// <summary>
            /// Override the shard.
            /// Can use for scenario like routing between environment like dev vs. prod or aws vs azure.
            /// </summary>
            /// <param name="shard">The shard.</param>
            /// <param name="type">The type.</param>
            /// <returns></returns>
            IProducerOverrideBuildBuilder<T> IProducerOverrideShardBuilder<T>.Shard(string shard, RouteAssignmentType type)
            {
                var plan = _plan.WithShard(shard, type);
                return new Router<T>(plan);
            }

            #endregion // Shard
        }

        #endregion // Router
    }
}
