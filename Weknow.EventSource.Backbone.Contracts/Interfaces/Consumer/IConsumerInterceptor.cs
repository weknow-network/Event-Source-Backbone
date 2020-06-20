using System;
using System.Collections.Immutable;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Consumer stage of an interception operation provider.
    /// It can be use for variety of responsibilities like 
    /// flowing auth context or traces, producing metrics, etc.
    /// </summary>
    /// <seealso cref="Weknow.EventSource.Backbone.IInterceptorName" />
    public interface IConsumerInterceptor<T>:
                                    IInterceptorName
                                where T : notnull
    {
        /// <summary>
        /// Interception operation.
        /// </summary>
        /// <param name="announcement"></param>
        /// <param name="interceptorData">
        /// The interceptor data which sets on the 
        /// producer stage of the interception.</param>
        void Intercept(
                   Announcement<T> announcement,
                   ReadOnlyMemory<byte> interceptorData);
    }
}