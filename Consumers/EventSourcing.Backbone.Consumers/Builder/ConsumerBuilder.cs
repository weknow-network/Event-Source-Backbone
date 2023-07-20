using System.Buffers;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

using EventSourcing.Backbone.Building;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Polly;

using static EventSourcing.Backbone.EventSourceConstants;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Event Source consumer builder.
    /// </summary>
    [DebuggerDisplay("{_plan.Environment}:{_plan.Uri}")]
    public partial class ConsumerBuilder :
        IConsumerBuilder,
        IConsumerReadyBuilder,
        IConsumerStoreStrategyBuilder,
        IConsumerIocStoreStrategyBuilder
    {
        private readonly ConsumerPlan _plan = ConsumerPlan.Empty;

        /// <summary>
        /// Event Source consumer builder.
        /// </summary>
        public static readonly IConsumerBuilder Empty = new ConsumerBuilder();

        /// <summary>
        /// Event Source consumer builder.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns></returns>
        public static IConsumerBuilder Create(IServiceProvider serviceProvider) => new ConsumerBuilder(serviceProvider);

        #region Ctor

        /// <summary>
        /// Prevents a default instance of the <see cref="ConsumerBuilder"/> class from being created.
        /// </summary>
        private ConsumerBuilder()
        {

        }

        /// <summary>
        /// Prevents a default instance of the <see cref="ConsumerBuilder" /> class from being created.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        private ConsumerBuilder(IServiceProvider serviceProvider)
        {
            TelemetryLevel? telemetryLevel = serviceProvider?.GetService<TelemetryLevel>();
            if (telemetryLevel != null)
            {
                var optins = _plan.Options with { TelemetryLevel = telemetryLevel };
                _plan = _plan.WithOptions(optins);
            }
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="plan">The plan.</param>
        private ConsumerBuilder(ConsumerPlan plan)
        {
            _plan = plan;
        }

        #endregion // Ctor

        #region Route

        /// <summary>
        /// The routing information attached to this buildr
        /// </summary>
        IPlanRoute IConsumerSubscribeBuilder.Route => _plan;

        #endregion // Route

        #region UseChannel

        /// <summary>
        /// Choose the communication channel provider.
        /// </summary>
        /// <param name="channel">The channel provider.</param>
        /// <returns></returns>
        IConsumerStoreStrategyBuilder IConsumerBuilder.UseChannel(
                        Func<ILogger, IConsumerChannelProvider> channel)
        {
            var prms = _plan.WithChannelFactory(channel);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        /// <summary>
        /// Choose the communication channel provider.
        /// </summary>
        /// <param name="serviceProvider">Dependency injection provider.</param>
        /// <param name="channel">The channel provider.</param>
        /// <returns></returns>
        IConsumerIocStoreStrategyBuilder IConsumerBuilder.UseChannel(
                        IServiceProvider serviceProvider,
                        Func<ILogger, IConsumerChannelProvider> channel)
        {
            var prms = _plan.WithChannelFactory(channel, serviceProvider);
            var result = new ConsumerBuilder(prms);
            return result;
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
        /// <returns></returns>
        IConsumerIocStoreStrategyBuilder IConsumerStoreStrategyBuilder<IConsumerIocStoreStrategyBuilder>.AddStorageStrategyFactory(
            Func<ILogger, IConsumerStorageStrategy> storageStrategy, EventBucketCategories targetType)
        {
            return AddStorageStrategyFactory(storageStrategy, targetType);
        }

        /// <summary>
        /// Adds the storage strategy (Segment / Interceptions).
        /// Will use default storage (REDIS Hash) when empty.
        /// When adding more than one it will to all, act as a fall-back (first win, can use for caching).
        /// It important the consumer's storage will be in sync with this setting.
        /// </summary>
        /// <param name="storageStrategy">Storage strategy provider.</param>
        /// <param name="category">Type of the target.</param>
        /// <returns></returns>
        IConsumerStoreStrategyBuilder IConsumerStoreStrategyBuilder<IConsumerStoreStrategyBuilder>.AddStorageStrategyFactory(
            Func<ILogger, IConsumerStorageStrategy> storageStrategy,
            EventBucketCategories category)
        {
            return AddStorageStrategyFactory(storageStrategy, category);
        }

        /// <summary>
        /// Adds the storage strategy factory.
        /// </summary>
        /// <param name="storageStrategy">The storage strategy.</param>
        /// <param name="category">Type of the category.</param>
        /// <returns></returns>
        private ConsumerBuilder AddStorageStrategyFactory(
            Func<ILogger, IConsumerStorageStrategy> storageStrategy,
            EventBucketCategories category)
        {

            var prms = _plan.WithStorageStrategy(Local);
            var result = new ConsumerBuilder(prms);

            return result;

            IConsumerStorageStrategyWithFilter Local(ILogger logger)
            {
                var strategy = storageStrategy(logger);
                var decorated = new FilteredStorageStrategy(strategy, category);
                return decorated;
            }
        }

        #endregion // AddStorageStrategy

        #region WithOptions

        /// <summary>
        /// Attach configuration.
        /// </summary>
        /// <param name="optionsStrategy"></param>
        /// <returns></returns>
        IConsumerHooksBuilder IConsumerOptionsBuilder.WithOptions(
            Func<ConsumerOptions, ConsumerOptions>? optionsStrategy)
        {
            if (optionsStrategy == null)
                return this;

            var options = optionsStrategy(_plan.Options);
            var prms = _plan.WithOptions(options);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        /// <summary>
        /// Attach configuration.
        /// </summary>
        /// <param name="optionsStrategy"></param>
        /// <returns></returns>
        IConsumerSubscribeBuilder IConsumerSubscribeBuilder.WithOptions(Func<ConsumerOptions, ConsumerOptions> optionsStrategy)
        {
            var options = optionsStrategy(_plan.Options);
            var prms = _plan.WithOptions(options);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        #endregion // WithOptions

        #region Environment

        /// <summary>
        /// Include the environment as prefix of the stream key.
        /// for example: env:URI
        /// </summary>
        /// <param name="environment">The environment (null: keep current environment, empty: reset the environment to nothing).</param>
        /// <returns></returns>
        IConsumerUriBuilder<IConsumerReadyBuilder> IConsumerEnvironmentOfBuilder<IConsumerUriBuilder<IConsumerReadyBuilder>>.Environment(Env? environment)
        {
            if (environment == null)
                return this;

            var prms = _plan.WithEnvironment(environment);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        /// <summary>
        /// Include the environment as prefix of the stream key.
        /// for example: env:URI
        /// </summary>
        /// <param name="environment">The environment (null: keep current environment, empty: reset the environment to nothing).</param>
        /// <returns></returns>
        IConsumerSubscribeBuilder IConsumerEnvironmentOfBuilder<IConsumerSubscribeBuilder>.Environment(Env? environment)
        {
            if (environment == null)
                return this;

            var prms = _plan.WithEnvironment(environment);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        #endregion // Environment

        #region EnvironmentFromVariable

        /// <summary>
        /// Fetch the origin environment of the message from an environment variable.
        /// </summary>
        /// <param name="environmentVariableKey">The environment variable key.</param>
        /// <returns></returns>
        /// <exception cref="EventSourcing.Backbone.EventSourcingException">EnvironmentFromVariable failed, [{environmentVariableKey}] not found!</exception>
        IConsumerUriBuilder<IConsumerReadyBuilder> IConsumerEnvironmentOfBuilder<IConsumerUriBuilder<IConsumerReadyBuilder>>.EnvironmentFromVariable(string environmentVariableKey)
        {
            string environment = Environment.GetEnvironmentVariable(environmentVariableKey) ?? throw new EventSourcingException($"EnvironmentFromVariable failed, [{environmentVariableKey}] not found!");

            var prms = _plan.WithEnvironment(environment);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        /// <summary>
        /// Fetch the origin environment of the message from an environment variable.
        /// </summary>
        /// <param name="environmentVariableKey">The environment variable key.</param>
        /// <returns></returns>
        /// <exception cref="EventSourcing.Backbone.EventSourcingException">EnvironmentFromVariable failed, [{environmentVariableKey}] not found!</exception>
        IConsumerSubscribeBuilder IConsumerEnvironmentOfBuilder<IConsumerSubscribeBuilder>.EnvironmentFromVariable(string environmentVariableKey)
        {
            string environment = Environment.GetEnvironmentVariable(environmentVariableKey) ?? throw new EventSourcingException($"EnvironmentFromVariable failed, [{environmentVariableKey}] not found!");

            var prms = _plan.WithEnvironment(environment);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        #endregion // EnvironmentFromVariable

        #region ServiceProvider

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        /// <exception cref="EventSourcing.Backbone.EventSourcingException">ServiceProvider is null</exception>
        IServiceProvider IConsumerIocStoreStrategyBuilder.ServiceProvider => _plan.ServiceProvider ?? throw new EventSourcingException("ServiceProvider is null");

        #endregion // ServiceProvider

        #region Uri

        /// <summary>
        /// Stream's name
        /// </summary>
        /// <param name="uri">
        /// The stream identifier (the URI combined with the environment separate one stream from another)
        /// </param>
        /// <returns></returns>
        IConsumerReadyBuilder IConsumerUriBuilder<IConsumerReadyBuilder>.Uri(string uri)
        {
            var prms = _plan.WithKey(uri);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        /// <summary>
        /// Stream's name
        /// </summary>
        /// <param name="uri">
        /// The stream identifier (the URI combined with the environment separate one stream from another)
        /// </param>
        /// <returns></returns>
        IConsumerSubscribeBuilder IConsumerUriBuilder<IConsumerSubscribeBuilder>.Uri(
                                    string uri)
        {
            var prms = _plan.WithKey(uri);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        #endregion // Uri

        #region RegisterSegmentationStrategy

        /// <summary>
        /// Responsible of building instance from segmented data.
        /// Segmented data is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="segmentationStrategy">The segmentation strategy.</param>
        /// <returns></returns>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        IConsumerHooksBuilder IConsumerHooksBuilder.RegisterSegmentationStrategy(IConsumerSegmentationStrategy segmentationStrategy)
        {
            var bridge = new ConsumerSegmentationStrategyBridge(segmentationStrategy);
            var prms = _plan.AddSegmentation(bridge);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        /// <summary>
        /// Responsible of building instance from segmented data.
        /// Segmented data is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="segmentationStrategy">The segmentation strategy.</param>
        /// <returns></returns>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        IConsumerHooksBuilder IConsumerHooksBuilder.RegisterSegmentationStrategy(IConsumerAsyncSegmentationStrategy segmentationStrategy)
        {
            var prms = _plan.AddSegmentation(segmentationStrategy);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        #endregion // RegisterSegmentationStrategy

        #region RegisterInterceptor

        /// <summary>
        /// Registers the interceptor.
        /// </summary>
        /// <param name="interceptorData">The interceptor.</param>
        /// <returns></returns>
        IConsumerHooksBuilder IConsumerHooksBuilder.RegisterInterceptor(
                            IConsumerInterceptor interceptorData)
        {
            var bridge = new ConsumerInterceptorBridge(interceptorData);
            var prms = _plan.AddInterceptor(bridge);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        /// <summary>
        /// Registers the interceptor.
        /// </summary>
        /// <param name="interceptorData">The interceptor.</param>
        /// <returns></returns>
        IConsumerHooksBuilder IConsumerHooksBuilder.RegisterInterceptor(
                            IConsumerAsyncInterceptor interceptorData)
        {
            var prms = _plan.AddInterceptor(interceptorData);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        #endregion // RegisterInterceptor

        #region WithLogger

        /// <summary>
        /// Attach logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        IConsumerSubscribeBuilder IConsumerReadyBuilder.WithLogger(ILogger logger)
        {
            var prms = _plan.WithLogger(logger);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        #endregion // WithLogger

        #region WithResiliencePolicy

        /// <summary>
        /// Set resilience policy
        /// </summary>
        /// <param name="policy">The policy.</param>
        /// <returns></returns>
        IConsumerReadyBuilder IConsumerReadyBuilder.WithResiliencePolicy(AsyncPolicy policy)
        {
            var prms = _plan.WithResiliencePolicy(policy);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        #endregion // WithResiliencePolicy

        #region Group

        /// <summary>
        /// Groups the specified consumer group.
        /// </summary>
        /// <param name="consumerGroup">The consumer group.</param>
        /// <returns></returns>
        IConsumerSubscribeBuilder IConsumerSubscribeBuilder.Group(string consumerGroup)
        {
            ConsumerPlan plan = _plan.WithConsumerGroup(consumerGroup);
            return new ConsumerBuilder(plan);
        }

        #endregion // Group

        #region Name

        /// <summary>
        /// Names the specified consumer name.
        /// </summary>
        /// <param name="consumerName">Name of the consumer.</param>
        /// <returns></returns>
        IConsumerSubscribeBuilder IConsumerSubscribeBuilder.Name(string consumerName)
        {
            ConsumerPlan plan = _plan.WithConsumerName(consumerName);
            return new ConsumerBuilder(plan);
        }

        #endregion // Name

        #region Subscribe

        /// <summary>
        /// Subscribe consumer.
        /// </summary>
        /// <param name="handler">Per operation invocation handler, handle methods calls.</param>
        /// <returns>
        /// The subscription lifetime (dispose to remove the subscription)
        /// </returns>
        IConsumerLifetime IConsumerSubscriptionHubBuilder.Subscribe(ISubscriptionBridge handler)

        {
            return ((IConsumerSubscriptionHubBuilder)this).Subscribe(handler.ToEnumerable());
        }

        /// <summary>
        /// Subscribe consumer.
        /// </summary>
        /// <param name="handlers">Per operation invocation handler, handle methods calls.</param>
        /// <returns>
        /// The subscription lifetime (dispose to remove the subscription)
        /// </returns>
        IConsumerLifetime IConsumerSubscriptionHubBuilder.Subscribe(ISubscriptionBridge[] handlers)

        {
            return ((IConsumerSubscriptionHubBuilder)this).Subscribe(handlers as IEnumerable<ISubscriptionBridge>);
        }

        /// <summary>
        /// Subscribe consumer.
        /// </summary>
        /// <param name="handlers">Per operation invocation handler, handle methods calls.</param>
        /// <returns>
        /// The subscription lifetime (dispose to remove the subscription)
        /// </returns>
        IConsumerLifetime IConsumerSubscriptionHubBuilder.Subscribe(
            IEnumerable<ISubscriptionBridge> handlers)

        {
            #region Validation

            if (_plan == null)
                throw new EventSourcingException(nameof(_plan));

            #endregion // Validation


            ConsumerPlan plan = WithGroupIfEmpty(_plan);
            if (plan.SegmentationStrategies.Count == 0)
                plan = plan.AddSegmentation(new ConsumerDefaultSegmentationStrategy());

            var consumer = new ConsumerBase(plan, handlers);
            var subscription = consumer.Subscribe();
            return subscription;
        }

        /// <summary>
        /// Subscribe consumer.
        /// </summary>
        /// <param name="handlers">Per operation invocation handler, handle methods calls.</param>
        /// <returns>
        /// The subscription lifetime (dispose to remove the subscription)
        /// </returns>
        IConsumerLifetime IConsumerSubscriptionHubBuilder.Subscribe(
            params Func<Announcement, IConsumerBridge, Task<bool>>[] handlers)

        {
            return ((IConsumerSubscriptionHubBuilder)this).Subscribe(handlers as IEnumerable<Func<Announcement, IConsumerBridge, Task<bool>>>);
        }

        /// <summary>
        /// Subscribe consumer.
        /// </summary>
        /// <param name="handlers">Per operation invocation handler, handle methods calls.</param>
        /// <returns>
        /// The subscription lifetime (dispose to remove the subscription)
        /// </returns>
        IConsumerLifetime IConsumerSubscriptionHubBuilder.Subscribe(
            IEnumerable<Func<Announcement, IConsumerBridge, Task<bool>>> handlers)

        {
            #region Validation

            if (_plan == null)
                throw new EventSourcingException(nameof(_plan));

            #endregion // Validation

            ConsumerPlan plan = WithGroupIfEmpty(_plan);
            if (plan.SegmentationStrategies.Count == 0)
                plan = plan.AddSegmentation(new ConsumerDefaultSegmentationStrategy());

            var consumer = new ConsumerBase(plan, handlers);
            var subscription = consumer.Subscribe();
            return subscription;
        }

        #endregion // Subscribe

        #region Fallback

        /// <summary>
        /// Fallback the specified action.
        /// </summary>
        /// <param name="onFallback">The fallback's action.</param>
        /// <returns></returns>
        IConsumerSubscriptionHubBuilder IConsumerSubscribeBuilder.Fallback(
            Func<IConsumerFallback, Task> onFallback)
        {
            var prms = _plan.WithFallback(onFallback);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        #endregion // Fallback

        #region WithGroupIfEmpty

        /// <summary>
        /// Add consumer group if empty.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <returns></returns>
        private ConsumerPlan WithGroupIfEmpty(ConsumerPlan plan)
        {
            if (plan.ConsumerGroup != null) return plan;
            var consumerGroup = $"{DateTime.UtcNow:yyyy-MM-dd HH_mm} {Guid.NewGuid():N}";
            return plan.WithConsumerGroup(consumerGroup);
        }

        #endregion // WithGroupIfEmpty

        #region WithCancellation

        /// <summary>
        /// Withes the cancellation token.
        /// </summary>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        IConsumerHooksBuilder IWithCancellation<IConsumerHooksBuilder>.WithCancellation(CancellationToken cancellation)
        {
            var prms = _plan.WithCancellation(cancellation);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        /// <summary>
        /// Withes the cancellation token.
        /// </summary>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        IConsumerSubscribeBuilder IWithCancellation<IConsumerSubscribeBuilder>.WithCancellation(CancellationToken cancellation)
        {
            var prms = _plan.WithCancellation(cancellation);
            var result = new ConsumerBuilder(prms);
            return result;
        }

        #endregion // WithCancellation

        #region BuildReceiver

        /// <summary>
        /// Build receiver (on demand data query).
        /// </summary>
        /// <returns></returns>
        IConsumerReceiver IConsumerSubscribeBuilder.BuildReceiver() => new Receiver((_plan as IConsumerPlanBuilder).Build());

        #endregion // BuildReceiver

        #region BuildIterator

        /// <summary>
        /// Build iterator (pull fusion).
        /// </summary>
        /// <returns></returns>
        IConsumerIterator IConsumerSubscribeBuilder.BuildIterator()
        {
            var rawPlan = _plan;
            if (rawPlan.SegmentationStrategies.Count == 0)
                rawPlan = rawPlan.AddSegmentation(new ConsumerDefaultSegmentationStrategy());

            var plan = (rawPlan as IConsumerPlanBuilder).Build();

            var iterator = new Iterator(plan);
            return iterator;
        }

        #endregion // BuildIterator

        #region ToJson

        /// <summary>
        /// Converts to json.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="announcement">The announcement.</param>
        /// <param name="ignoreMetadata">if set to <c>true</c> [ignore metadata].</param>
        /// <returns></returns>
        /// <exception cref="System.DataMisalignedException"></exception>
        private static JsonElement ToJson(
                                   IConsumerPlan plan,
                                   AnnouncementData announcement,
                                   bool ignoreMetadata = false)
        {
            EventKey entryId = announcement.EventKey;
            var buffer = new ArrayBufferWriter<byte>();
            var options = new JsonWriterOptions();
            using (var w = new Utf8JsonWriter(buffer, options))
            {
                w.WriteStartObject();

                w.WritePropertyName("__event-key__");
                w.WriteStringValue(entryId);

                if (!ignoreMetadata)
                {
                    w.WritePropertyName("__message-id__");
                    w.WriteStringValue(announcement.MessageId);

                    w.WritePropertyName("__env__");
                    w.WriteStringValue(announcement.Environment);

                    w.WritePropertyName("__uri__");
                    w.WriteStringValue(announcement.Uri);

                    w.WritePropertyName("__operation__");
                    w.WriteStringValue(announcement.Operation);
                }

                foreach (KeyValuePair<string, ReadOnlyMemory<byte>> entry in announcement.Data)
                {
                    var (key, val) = entry;
                    JsonElement element;
                    try
                    {

                        element = plan.Options.Serializer.Deserialize<JsonElement>(val);
                    }
                    #region Exception Handling

                    catch (Exception ex)
                    {
                        string encoded = string.Empty;
                        try
                        {
                            encoded = Encoding.UTF8.GetString(val.Span);
                        }
                        catch
                        {
                            plan.Logger.LogDebug("Failed to encode value of ({key})", key);
                        }

                        var err = $"GetJsonByIdAsync [{entryId}, {announcement.FullUri()}]: failed to deserialize key='{key}', base64='{Convert.ToBase64String(val.ToArray())}', data={encoded}";
                        plan.Logger.LogError(ex.FormatLazy(), "GetJsonByIdAsync [{id}, {at}]: failed to deserialize key='{key}', base64='{value}', data={data}",
                            entryId, announcement.FullUri(), key,
                            Convert.ToBase64String(val.ToArray()),
                            encoded);
                        throw new DataMisalignedException(err, ex);
                    }

                    #endregion // Exception Handling
                    w.WritePropertyName(key);
                    element.WriteTo(w);
                }

                w.WriteEndObject();
            }
            var result = JsonSerializer.Deserialize<JsonElement>(
                                                    buffer.WrittenSpan,
                                                    SerializerOptionsWithIndent);
            return result;
        }

        /// <summary>
        /// Converts to json.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="announcement">The announcement.</param>
        /// <param name="ignoreMetadata">if set to <c>true</c> [ignore metadata].</param>
        /// <returns></returns>
        /// <exception cref="System.DataMisalignedException"></exception>
        private static JsonElement ToJson(
                                    IConsumerPlan plan,
                                    Announcement announcement,
                                   bool ignoreMetadata = false)
        {
            EventKey entryId = announcement.Metadata.EventKey;
            var buffer = new ArrayBufferWriter<byte>();
            var options = new JsonWriterOptions();
            using (var w = new Utf8JsonWriter(buffer, options))
            {
                w.WriteStartObject();

                if (!ignoreMetadata)
                {
                    w.WritePropertyName("__meta__");
                    announcement.Metadata.ToJson().WriteTo(w);
                }

                foreach (KeyValuePair<string, ReadOnlyMemory<byte>> entry in announcement.Segments)
                {
                    var (key, val) = entry;
                    JsonElement element;
                    try
                    {

                        element = plan.Options.Serializer.Deserialize<JsonElement>(val);
                    }
                    #region Exception Handling

                    catch (Exception ex)
                    {
                        string encoded = string.Empty;
                        try
                        {
                            encoded = Encoding.UTF8.GetString(val.Span);
                        }
                        #region Exception Handling

                        catch
                        {
                            plan.Logger.LogDebug("Encoding failure");
                        }

                        #endregion // Exception Handling

                        var err = $"GetJsonByIdAsync [{entryId}, {announcement.Metadata.FullUri()}]: failed to deserialize key='{key}', base64='{Convert.ToBase64String(val.ToArray())}', data={encoded}";
                        plan.Logger.LogError(ex.FormatLazy(), "GetJsonByIdAsync [{id}, {at}]: failed to deserialize key='{key}', base64='{value}', data={data}",
                            entryId, announcement.Metadata.FullUri(), key,
                            Convert.ToBase64String(val.ToArray()),
                            encoded);
                        throw new DataMisalignedException(err, ex);
                    }

                    #endregion // Exception Handling
                    w.WritePropertyName(key);
                    element.WriteTo(w);
                }

                w.WriteEndObject();
            }
            var result = JsonSerializer.Deserialize<JsonElement>(
                                                    buffer.WrittenSpan,
                                                    SerializerOptionsWithIndent);
            return result;
        }

        #endregion // ToJson
    }
}
