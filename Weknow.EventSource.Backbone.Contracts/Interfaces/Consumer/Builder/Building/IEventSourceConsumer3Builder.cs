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
    public interface IEventSourceConsumer3Builder<T> 
        where T: notnull
    {
        IEventSourceConsumer3Builder<T> AddInterceptor(
            Action<Announcement<T>> intercept);

        IEventSourceConsumer3Builder<T> AddAsyncInterceptor(
            Func<Announcement<T>, ValueTask> intercept);

        ISourceBlock<Ackable<Announcement<T>>> Build();
    }
}
