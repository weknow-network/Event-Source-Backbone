using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    public class RedisConsumerChannel : IConsumerChannelProvider
    {
        public RedisConsumerChannel()
        {

        }

        public Task Completion => throw new NotImplementedException();

        public void Complete()
        {
            throw new NotImplementedException();
        }

        public Ackable<AnnouncementRaw> ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<Ackable<AnnouncementRaw>> target, out bool messageConsumed)
        {
            throw new NotImplementedException();
        }

        public void Fault(Exception exception)
        {
            throw new NotImplementedException();
        }

        public IDisposable LinkTo(ITargetBlock<Ackable<AnnouncementRaw>> target, DataflowLinkOptions linkOptions)
        {
            throw new NotImplementedException();
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<Ackable<AnnouncementRaw>> target)
        {
            throw new NotImplementedException();
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<Ackable<AnnouncementRaw>> target)
        {
            throw new NotImplementedException();
        }
    }
}
