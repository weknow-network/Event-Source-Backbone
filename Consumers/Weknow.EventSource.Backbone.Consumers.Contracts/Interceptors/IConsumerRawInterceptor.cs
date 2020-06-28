using System;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Consumer stage of an interception operation provider.
    /// It can be use for variety of responsibilities like 
    /// flowing auth context or traces, producing metrics, etc.
    /// </summary>
    /// <seealso cref="Weknow.EventSource.Backbone.IInterceptorName" />
    public interface IConsumerRawInterceptor :
                                        IInterceptorName
    {
        /// <summary>
        /// Interception operation.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="interceptorData">
        /// The interceptor data which sets on the 
        /// producer stage of the interception.</param>
        void Intercept(
                    AnnouncementMetadata metadata,
                    ReadOnlyMemory<byte> interceptorData);
    }
}