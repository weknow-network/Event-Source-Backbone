using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Bucket = System.Collections.Immutable.ImmutableDictionary<string, System.ReadOnlyMemory<byte>>;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Handle the producing pipeline
    /// </summary>
    public class ProducerBase
    {
        private readonly string _partition;
        private readonly string _shard;
        private readonly IEventSourceOptions _options;
        private readonly IImmutableList<IProducerAsyncSegmentationStrategy> _segmentations;
        private readonly IProducerChannelProvider _channel;
        private readonly IImmutableList<IProducerAsyncInterceptor> _interceptors;

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
            _interceptors = parameters.Interceptors ?? ImmutableList<IProducerAsyncInterceptor>.Empty; ;
        }

        #endregion // Ctor

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

        #region SendAsync

        /// <summary>
        /// Sends the produced data via the channel.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="segments">The segments.</param>
        /// <returns></returns>
        protected async ValueTask SendAsync(
            string operation,
            ValueTask<Bucket>[] segments)
        {
            var payload = Bucket.Empty;
            foreach (var segmentFuture in segments)
            {
                Bucket? segmentation = await segmentFuture;
                payload = payload.AddRange(segmentation);
            }

            string id = Guid.NewGuid().ToString();
            Metadata metadata = new Metadata(id, _partition, _shard, operation);
            
            var interceptorsData = Bucket.Empty;
            foreach (IProducerAsyncInterceptor interceptor in _interceptors)
            {
                ReadOnlyMemory<byte> interceptorData = await  interceptor.InterceptAsync(metadata, payload);
                interceptorsData = interceptorsData.Add(
                                        interceptor.InterceptorName,
                                        interceptorData);
            }

            var announcment = new Announcement(
                metadata,
                payload,
                interceptorsData);
            await _channel.SendAsync(announcment);
        }

        #endregion // SendAsync
    }
}
