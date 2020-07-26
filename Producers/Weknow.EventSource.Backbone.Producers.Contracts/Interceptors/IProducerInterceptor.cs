using System;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Producer stage of an interception operation provider.
    /// It can be use for variety of responsibilities like 
    /// flowing auth context or traces, producing metrics, etc.
    /// </summary>
    /// <seealso cref="Weknow.EventSource.Backbone.IInterceptorName" />
    public interface IProducerInterceptor :
                                        IInterceptorName
    {
        /// <summary>
        /// Interception operation.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <returns>Data which will be available to the 
        /// consumer stage of the interception.</returns>
        ReadOnlyMemory<byte> Intercept(
                                Metadata metadata);
    }
}