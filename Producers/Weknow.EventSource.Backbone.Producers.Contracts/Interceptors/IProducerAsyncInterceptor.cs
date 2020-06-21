using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Producer stage of an interception operation provider.
    /// It can be use for variety of responsibilities like 
    /// flowing auth context or traces, producing metrics, etc.
    /// </summary>
    /// <seealso cref="Weknow.EventSource.Backbone.IInterceptorName" />
    public interface IProducerAsyncInterceptor<T> :
                                        IInterceptorName
                                    where T: notnull
    {
        /// <summary>
        /// Interception operation.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="announcement">The announcement.</param>
        /// <returns>Data which will be available to the 
        /// consumer stage of the interception.</returns>
        ValueTask<ReadOnlyMemory<byte>> Intercept(
                                AnnouncementMetadata metadata, T announcement);
    }
}