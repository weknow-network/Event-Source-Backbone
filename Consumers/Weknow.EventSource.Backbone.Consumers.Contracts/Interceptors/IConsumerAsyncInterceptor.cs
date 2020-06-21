using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Consumer stage of an interception operation provider.
    /// It can be use for variety of responsibilities like 
    /// flowing auth context or traces, producing metrics, etc.
    /// </summary>
    /// <seealso cref="Weknow.EventSource.Backbone.IInterceptorName" />
    public interface IConsumerAsyncInterceptor<T>:
                                IInterceptorName
                            where T: notnull
    {
        /// <summary>
        /// Interception operation.
        /// </summary>
        /// <param name="announcement"></param>
        /// <param name="interceptorData">
        /// The interceptor data which sets on the 
        /// producer stage of the interception.</param>
        Task InterceptAsync(
                    Announcement<T> announcement,
                    ReadOnlyMemory<byte> interceptorData);
    }
}