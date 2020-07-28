using System;
using System.Collections.Immutable;
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

        #region SendAsync

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operation">The operation.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="producedData">The produced data.</param>
        /// <returns></returns>
        protected async ValueTask SendAsync<T>(
            string operation,
            string argumentName,
            T producedData)
             where T : notnull
        {
            var segments = Bucket.Empty;
            foreach (IProducerAsyncSegmentationStrategy strategy in _segmentations)
            {
                segments = await strategy.ClassifyAsync(
                    segments,
                    operation,
                    argumentName,
                    producedData,
                    _options);
            }

            string id = Guid.NewGuid().ToString();
            Metadata metadata = new Metadata(id, _partition, _shard, operation);
            
            var interceptorsData = Bucket.Empty;
            foreach (IProducerAsyncInterceptor interceptor in _interceptors)
            {
                var interceptorData = await  interceptor.InterceptAsync(metadata, segments);
                interceptorsData = interceptorsData.Add(
                                        interceptor.InterceptorName,
                                        interceptorData);
            }

            var announcment = new Announcement(
                metadata,
                segments,
                interceptorsData);
            await _channel.SendAsync(announcment);
        }

        #endregion // SendAsync
    }
}
