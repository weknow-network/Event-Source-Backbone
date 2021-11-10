using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{

    /// <summary>
    /// common plan properties
    /// </summary>
    public interface IProducerPlanBase : IPlanRoute
    {
        /// <summary>
        /// Producer interceptors (Timing: after serialization).
        /// </summary>
        /// <value>
        /// The interceptors.
        /// </value>
        IImmutableList<IProducerAsyncInterceptor> Interceptors { get; }
        /// <summary>
        /// Gets the logger.
        /// </summary>
        ILogger Logger { get; }
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        EventSourceOptions Options { get; }
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
        IImmutableList<IProducerAsyncSegmentationStrategy> SegmentationStrategies { get; }
    }
}