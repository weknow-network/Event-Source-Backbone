using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;

using Bucket = System.Collections.Immutable.ImmutableDictionary<string, System.ReadOnlyMemory<byte>>;


namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Handle the producing pipeline
    /// </summary>
    public abstract class ProducerBase
    {
        private readonly string _partition;
        private readonly string _shard;
        private readonly IEventSourceOptions _options;
        private readonly IImmutableList<IProducerAsyncSegmentationStrategy> _segmentations;
        private readonly IProducerChannelProvider _channel;
        private readonly IImmutableList<IProducerAsyncInterceptor> _interceptors;
        private readonly IImmutableList<IProducerHooksBuilder> _forwards;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        public ProducerBase(ProducerParameters parameters)
        {
            _options = parameters.Options;
            _partition = parameters.Partition;
            _shard = parameters.Shard;
            _segmentations = parameters.SegmentationStrategies;
            _channel = parameters.Channel ?? throw new ArgumentNullException(nameof(parameters.Channel)); ;
            _interceptors = parameters.Interceptors ?? ImmutableList<IProducerAsyncInterceptor>.Empty;
            _forwards = parameters.Forwards ?? ImmutableList<IProducerHooksBuilder>.Empty;
        }

        #endregion // Ctor

        // TODO: [bnaya, 2020-07] Review with AVI,  base class should call abstract method on call in order to support merge, 

        protected abstract ValueTask<Bucket> DoSegmentation(
            Bucket segmentation,
            string operation);

        #region ClassifyAsync

        /// <summary>
        /// Classify the operation payload from method arguments.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        private async Task<Bucket> ClassifyAsync(string operation, Bucket payload)
        {
            foreach (var segmentFuture in _segmentations)
            {
                Bucket? segmentation = await DoSegmentation(payload, operation);
                payload = payload.AddRange(segmentation);
            }

            return payload;
        }

        #endregion // ClassifyAsync

        #region ClassifyArgumentAsync

        /// <summary>
        /// Prepare data of single argument in an operation
        /// for sending.
        /// By classifies the data into segments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operation">The operation.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="producedData">The produced data.</param>
        /// <returns></returns>
        protected async ValueTask<Bucket> ClassifyArgumentAsync<T>(
            string operation, 
            string argumentName,
            T producedData)
        {
            Bucket segments = Bucket.Empty;
            foreach (IProducerAsyncSegmentationStrategy strategy in _segmentations)
            {
                var seg = await strategy.TryClassifyAsync(
                    segments,
                    operation,
                    argumentName,
                    producedData,
                    _options);

                #region Validation

                if (seg == null)
                {
                    // TODO: Log warning $"{nameof(strategy.TryClassifyAsync)} don't expect to return null value");
                    continue;
                }

                #endregion // Validation
                segments = seg;
            }
            return segments;
        }

        #endregion // ClassifyArgumentAsync

        #region InterceptAsync

        /// <summary>
        /// Call interceptors and store their intercepted data 
        /// (which will be use by the consumer's interceptors).
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="interceptorsData">The interceptors data.</param>
        /// <returns></returns>
        private async Task<Bucket> InterceptAsync(Metadata metadata, Bucket payload, Bucket interceptorsData)
        {
            foreach (IProducerAsyncInterceptor interceptor in _interceptors)
            {
                ReadOnlyMemory<byte> interceptorData = await interceptor.InterceptAsync(metadata, payload);
                interceptorsData = interceptorsData.Add(
                                        interceptor.InterceptorName,
                                        interceptorData);
            }

            return interceptorsData;
        }

        #endregion // InterceptAsync

        #region SendAsync

        /// <summary>
        /// Sends the produced data via the channel.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <returns></returns>
        protected ValueTask SendAsync(
            string operation)
        {
            string id = Guid.NewGuid().ToString();
            Metadata metadata = new Metadata(id, _partition, _shard, operation);

            var payload = Bucket.Empty;
            var interceptorsData = Bucket.Empty;
            return SendAsync(
                        metadata,
                        payload, 
                        interceptorsData, 
                        operation);
        }

        /// <summary>
        /// Sends the produced data via the channel.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="interceptorsData">The interceptors data.</param>
        /// <param name="operation">The operation.</param>
        /// <returns></returns>
        protected async ValueTask SendAsync(
            Metadata metadata,
            Bucket payload,
            Bucket interceptorsData,
            string operation)
        {

            payload = await ClassifyAsync(operation, payload);

            interceptorsData = await InterceptAsync(metadata, payload, interceptorsData);

            var announcement = new Announcement(
                metadata,
                payload,
                interceptorsData);

            if (_forwards.Count == 0) // merged
            {
                await _channel.SendAsync(announcement);
                return;
            }

            foreach (var forward in _forwards)
            {   // merged scenario
                await SendAsync(
                            metadata,
                            payload,
                            interceptorsData,
                            operation);
            }
        }

        #endregion // SendAsync
    }
}
