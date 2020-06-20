using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    public interface IInterceptorName
    {
        /// <summary>
        /// Unique name which represent the correlation
        /// between the producer and consumer interceptor.
        /// It's recommended to use URL format.
        /// </summary>
        string InterceptorName { get; }
    }
}