using System.Threading.Tasks.Dataflow;

namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IEventSourceConsumer3Builder<T> 
        where T: notnull
    {
        IEventSourceConsumer3Builder<T> AddInterceptor(IConsumerInterceptor<T> interceptor);

        IEventSourceConsumer3Builder<T> AddAsyncInterceptor(IConsumerAsyncInterceptor<T> interceptor);

        ISourceBlock<Ackable<Announcement<T>>> Build();
    }
}
