using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

// TODO: consider Channel<T>

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Channel provider responsible for passing the actual message 
    /// from producer to consumer. 
    /// </summary>
    public interface IConsumerChannelProvider
    {
        void Init(IEventSourceConsumerOptions options);
        ValueTask ReceiveAsync(Func<Announcement, ValueTask> func);
    }
}