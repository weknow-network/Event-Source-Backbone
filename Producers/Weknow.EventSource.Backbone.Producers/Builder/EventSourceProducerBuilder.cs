using System;
using System.Collections;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public class EventSourceProducerBuilder : IEventSourceProducerOptionsBuilder
    {
        private readonly EventSourceOptions _options = EventSourceOptions.Empty;
        private readonly IImmutableQueue<IProducerRawAsyncInterceptor> _rawInterceptor =
                                        ImmutableQueue<IProducerRawAsyncInterceptor>.Empty;
        private readonly string channel = "main";

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSourceProducerBuilder"/> class.
        /// </summary>
        private EventSourceProducerBuilder()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSourceProducerBuilder" /> class.
        /// </summary>
        /// <param name="copyFrom">The copy from.</param>
        /// <param name="channel">The event source channel name.</param>
        /// <param name="options">The options.</param>
        /// <param name="interceptor">The interceptor.</param>
        private EventSourceProducerBuilder(
                    EventSourceProducerBuilder copyFrom,
                    string? channel = null,
                    EventSourceOptions? options = null,
                    IImmutableQueue<IProducerRawAsyncInterceptor>? interceptor = null)
        {
            this.channel = channel ?? copyFrom.channel;
            _options = options ?? copyFrom._options;
            _rawInterceptor = interceptor ?? copyFrom._rawInterceptor;
        }

        #endregion // Ctor

        #region WithOptions

        /// <summary>
        /// Gets the main event source.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IEventSourceProducerSpecializeBuilder IEventSourceProducerOptionsBuilder.WithOptions(
                                                    EventSourceOptions options)
        {
            return new EventSourceProducerBuilder(this, options: options);
        }

        #endregion // WithOptions

        #region AddAsyncInterceptor

        /// <summary>
        /// Adds the asynchronous interceptor.
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns></returns>
        IEventSourceProducerSpecializeBuilder IEventSourceProducerSpecializeBuilder.AddAsyncInterceptor(
                                                    IProducerRawAsyncInterceptor interceptor)
        {
            return new EventSourceProducerBuilder(
                this,
                interceptor: _rawInterceptor.Enqueue(interceptor));
        }

        #endregion // AddAsyncInterceptor

        #region AddInterceptor

        /// <summary>
        /// Adds the interceptor.
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IEventSourceProducerSpecializeBuilder IEventSourceProducerSpecializeBuilder.AddInterceptor(
                                                    IProducerRawInterceptor interceptor)
        {
            var asyncInterceptor = new ToRawAsyncInterceptor(interceptor);
            return new EventSourceProducerBuilder(
                this,
                interceptor: _rawInterceptor.Enqueue(asyncInterceptor));
        }

        #endregion // AddInterceptor

        #region ForEventType

        /// <summary>
        /// Define the Producer for payload type and default eventName.
        /// </summary>
        /// <typeparam name="T">The payload type</typeparam>
        /// <param name="defaultEventName">The event name is the operation key.
        /// It can stand for itself for simple event or be associate with typed payload.
        /// It's recommended not to use the payload type as the event name, 
        /// because the payload type should be change on each breaking change of the type
        /// in order to support multi versions.
        /// </param>
        /// <returns></returns>
        IEventSourceProducerDecoratorBuilder<T> IEventSourceProducerSpecializeBuilder.ForEventType<T>(
                                                            string defaultEventName)
        {
            return new EventSourceProducerBuilder<T>(this, defaultEventName);
        }

        /// <summary>
        /// Define the Producer with default eventName.
        /// </summary>
        /// <param name="defaultEventName">The event name is the operation key.</param>
        /// <returns></returns>
        IEventSourceProducerDecoratorBuilder<string> IEventSourceProducerSpecializeBuilder.ForEventType(
                                                            string defaultEventName)
        {
            return new EventSourceProducerBuilder<string>(this, defaultEventName);
        }

        #endregion // ForEventType

        #region ToRawAsyncInterceptor

        /// <summary>
        /// Wrap sync interceptor as async interceptor
        /// </summary>
        /// <seealso cref="Weknow.EventSource.Backbone.IProducerRawAsyncInterceptor" />
        private class ToRawAsyncInterceptor : IProducerRawAsyncInterceptor
        {
            private readonly IProducerRawInterceptor _interceptor;

            #region Ctor

            /// <summary>
            /// Initializes a new instance of the <see cref="ToRawAsyncInterceptor"/> class.
            /// </summary>
            /// <param name="interceptor">The interceptor.</param>
            public ToRawAsyncInterceptor(IProducerRawInterceptor interceptor)
            {
                _interceptor = interceptor;
            }

            #endregion // Ctor

            #region InterceptorName

            /// <summary>
            /// Unique name which represent the correlation
            /// between the producer and consumer interceptor.
            /// It's recommended to use URL format.
            /// </summary>
            public string InterceptorName => _interceptor.InterceptorName;

            #endregion // InterceptorName

            #region InterceptAsync

            /// <summary>
            /// Interception operation.
            /// </summary>
            /// <param name="metadata">The metadata.</param>
            /// <returns>
            /// Data which will be available to the
            /// consumer stage of the interception.
            /// </returns>
            public ValueTask<ReadOnlyMemory<byte>> InterceptAsync(AnnouncementMetadata metadata)
            {
                var result = _interceptor.Intercept(metadata);
                return result.ToValueTask();
            }

            #endregion // InterceptAsync
        }

        #endregion // ToRawAsyncInterceptor
    }

    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public class EventSourceProducerBuilder<T> : IEventSourceProducerDecoratorBuilder<T>
        where T:notnull
    {
        private readonly EventSourceProducerBuilder _basedOn;
        private readonly string _defaultEventName;
        private readonly IImmutableQueue<IProducerAsyncDecorator<T>> _typedInterceptor =
                                        ImmutableQueue<IProducerAsyncDecorator<T>>.Empty;
        private readonly IImmutableQueue<IProducerSegmenationStrategy<T>> _segmentations =
                                        ImmutableQueue<IProducerSegmenationStrategy<T>>.Empty;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSourceProducerBuilder"/> class.
        /// </summary>
        internal EventSourceProducerBuilder(
            EventSourceProducerBuilder basedOn,
            string defaultEventName)
        {
            _basedOn = basedOn;
            _defaultEventName = defaultEventName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSourceProducerBuilder" /> class.
        /// </summary>
        /// <param name="copyFrom">The copy from.</param>
        /// <param name="typedInterceptor">The typed interceptor.</param>
        /// <param name="segmentations">The segmentations.</param>
        public EventSourceProducerBuilder(
                    EventSourceProducerBuilder<T> copyFrom,
                    IImmutableQueue<IProducerAsyncDecorator<T>>? typedInterceptor = null,
                    IImmutableQueue<IProducerSegmenationStrategy<T>>? segmentations = null)
        {
            _basedOn = copyFrom._basedOn;
            _defaultEventName = copyFrom._defaultEventName;
            _typedInterceptor = typedInterceptor ?? copyFrom._typedInterceptor;
            _segmentations = segmentations ?? copyFrom._segmentations;
        }

        #endregion // Ctor

        #region AddAsyncInterceptor

        /// <summary>
        /// Adds the asynchronous interceptor.
        /// </summary>
        /// <param name="intercept">The intercept.</param>
        /// <returns></returns>
        IEventSourceProducerDecoratorBuilder<T> IEventSourceProducerDecoratorBuilder<T>.AddAsyncInterceptor(
                                                    IProducerAsyncDecorator<T> intercept)
        {
            IImmutableQueue<IProducerAsyncDecorator<T>> interceptors = 
                                            _typedInterceptor.Enqueue(intercept);
            return new EventSourceProducerBuilder<T>(this, typedInterceptor: interceptors);
        }

        #endregion // AddAsyncInterceptor

        #region AddInterceptor

        /// <summary>
        /// Adds the interceptor.
        /// </summary>
        /// <param name="intercept">The intercept.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IEventSourceProducerDecoratorBuilder<T> IEventSourceProducerDecoratorBuilder<T>.AddInterceptor(
                                                    IProducerDecorator<T> intercept)
        {
            IProducerAsyncDecorator<T> asyncInterceptor = new ToAsyncInterceptor(intercept);
            IImmutableQueue<IProducerAsyncDecorator<T>> interceptors = 
                                            _typedInterceptor.Enqueue(asyncInterceptor);
            return new EventSourceProducerBuilder<T>(this, typedInterceptor: interceptors);
        }

        #endregion // AddInterceptor

        #region AddSegmentationProvider

        /// <summary>
        /// Adds segmentation provider.
        /// Responsible of splitting an instance into segments.
        /// Segments is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="segmentationProvider"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        IEventSourceProducerFinalBuilder<T> IEventSourceProducerDecoratorBuilder<T>.AddSegmentationProvider(
                                                            IProducerSegmenationStrategy<T> segmentationProvider)
        {
            var segmentations = _segmentations.Enqueue(segmentationProvider);
            return new EventSourceProducerBuilder<T>(this, segmentations: segmentations);
        }

        /// <summary>
        /// Adds segmentation provider.
        /// Responsible of splitting an instance into segments.
        /// Segments is how the producer sending its raw data to
        /// the consumer. It's in a form of dictionary when
        /// keys represent the different segments
        /// and the value represent serialized form of the segment's data.
        /// </summary>
        /// <param name="segmentationProviderExpression"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <example>
        /// Examples for segments can be driven from regulation like
        /// GDPR (personal, non-personal data),
        /// Technical vs Business aspects, etc.
        /// </example>
        IEventSourceProducerFinalBuilder<T> IEventSourceProducerDecoratorBuilder<T>.AddSegmentationProvider(
                                                            Func<T, IDataSerializer, ImmutableDictionary<string, ReadOnlyMemory<byte>>> segmentationProviderExpression)
        {
            IProducerSegmenationStrategy<T> segmentationProvider = new ToSegmenationProvider(segmentationProviderExpression);
            var segmentations = _segmentations.Enqueue(segmentationProvider);
            return new EventSourceProducerBuilder<T>(this, segmentations: segmentations);
        }

        #endregion // AddSegmentationProvider

        /// <summary>
        /// Builds producer instance.
        /// </summary>
        /// <param name="sourceShard">Specify the event source shard.
        /// Shard it a unique source name
        /// for specific message channeling (routing).</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IEventSourceProducer<T> IEventSourceProducerFinalBuilder<T>.Build(string sourceShard)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds tags.
        /// Attach channel's tags which enable the consumer
        /// to get data from multiple sources.
        /// For example: assuming that each order flow is written to
        /// unique source.
        /// Tagging it with ORDER tag enables consumer
        /// to consume all orders flow (consume from multiple sources).
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IEventSourceProducerDecoratorBuilder<T> IEventSourceProducerDecoratorBuilder<T>.AddTags(params string[] tags)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds tags.
        /// Attach channel's tags which enable the consumer
        /// to get data from multiple sources.
        /// For example: assuming that each order flow is written to
        /// unique source.
        /// Tagging it with ORDER tag enables consumer
        /// to consume all orders flow (consume from multiple sources).
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IEventSourceProducerDecoratorBuilder<T> IEventSourceProducerDecoratorBuilder<T>.AddTags(IEnumerable[] tags)
        {
            throw new NotImplementedException();
        }

        #region ToAsyncInterceptor

        /// <summary>
        /// Wrap sync interceptor as async interceptor
        /// </summary>
        private class ToAsyncInterceptor : IProducerAsyncDecorator<T>
        {
            private readonly IProducerDecorator<T> _interceptor;

            #region Ctor

            /// <summary>
            /// Initializes a new instance of the <see cref="ToAsyncInterceptor"/> class.
            /// </summary>
            /// <param name="interceptor">The interceptor.</param>
            public ToAsyncInterceptor(IProducerDecorator<T> interceptor)
            {
                _interceptor = interceptor;
            }

            #endregion // Ctor

            #region InterceptorName

            /// <summary>
            /// Unique name which represent the correlation
            /// between the producer and consumer interceptor.
            /// It's recommended to use URL format.
            /// </summary>
            public string InterceptorName => _interceptor.InterceptorName;

            #endregion // InterceptorName

            #region InterceptAsync

            /// <summary>
            /// Interception operation.
            /// </summary>
            /// <param name="metadata">The metadata.</param>
            /// <param name="announcement">The announcement.</param>
            /// <returns>Data which will be available to the 
            /// consumer stage of the interception.</returns>
            public ValueTask<ReadOnlyMemory<byte>> Intercept(
                                    AnnouncementMetadata metadata, T announcement)
            {
                var result = _interceptor.Intercept(metadata, announcement);
                return result.ToValueTask();
            }

            #endregion // InterceptAsync
        }

        #endregion // ToAsyncInterceptor

        #region ToSegmenationProvider

        /// <summary>
        /// Wrap segmentation provider.
        /// </summary>
        private class ToSegmenationProvider : IProducerSegmenationStrategy<T>
        {
            private readonly Func<T, IDataSerializer, ImmutableDictionary<string, ReadOnlyMemory<byte>>> _segmentationProvider;

            #region Ctor

            /// <summary>
            /// Initializes a new instance of the <see cref="ToAsyncInterceptor" /> class.
            /// </summary>
            /// <param name="segmentationProvider">The segmentation provider.</param>
            public ToSegmenationProvider(Func<T, IDataSerializer, ImmutableDictionary<string, ReadOnlyMemory<byte>>> segmentationProvider)
            {
                _segmentationProvider = segmentationProvider;
            }

            #endregion // Ctor

            #region Classify

            /// <summary>
            /// Classifies instance into different segments.
            /// Segments is how the producer sending its raw data to
            /// the consumer. It's in a form of dictionary when
            /// keys represent the different segments
            /// and the value represent serialized form of the segment's data.
            /// </summary>
            /// <param name="producedData"></param>
            /// <param name="serializer"></param>
            /// <returns></returns>
            /// <exception cref="NotImplementedException"></exception>
            /// <example>
            /// Examples for segments can be driven from regulation like
            /// GDPR (personal, non-personal data),
            /// Technical vs Business aspects, etc.
            /// </example>
            public ImmutableDictionary<string, ReadOnlyMemory<byte>> Classify(
                                                        T producedData,
                                                        IDataSerializer serializer)
            {
                return _segmentationProvider(producedData, serializer);
            }

            #endregion // Classify
        }

        #endregion // ToSegmenationProvider
    }
}
