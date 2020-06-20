using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IEventSourceProducer2Builder
    {
        IEventSourceProducer2Builder AddInterceptor(
                                IProducerRawInterceptor interceptor);

        IEventSourceProducer2Builder AddAsyncInterceptor(
                                IProducerRawAsyncInterceptor interceptor);

        IEventSourceProducer3Builder<T> ForType<T>(string intent) 
                                        where T: notnull; 
    }
}
