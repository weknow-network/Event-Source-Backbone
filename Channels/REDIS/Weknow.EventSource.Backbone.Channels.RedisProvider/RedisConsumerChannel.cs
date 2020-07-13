using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Weknow.EventSource.Backbone.Channels.RedisProvider
{
    public class RedisConsumerChannel : IConsumerChannelProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RedisConsumerChannel()
        {

        }

        #endregion // Ctor

        Task IDataflowBlock.Completion => throw new NotImplementedException();

        void IDataflowBlock.Complete()
        {
            throw new NotImplementedException();
        }

        Ackable<AnnouncementRaw> ISourceBlock<Ackable<AnnouncementRaw>>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<Ackable<AnnouncementRaw>> target, out bool messageConsumed)
        {
            throw new NotImplementedException();
        }

        void IDataflowBlock.Fault(Exception exception)
        {
            throw new NotImplementedException();
        }

        IDisposable ISourceBlock<Ackable<AnnouncementRaw>>.LinkTo(ITargetBlock<Ackable<AnnouncementRaw>> target, DataflowLinkOptions linkOptions)
        {
            throw new NotImplementedException();
        }

        void ISourceBlock<Ackable<AnnouncementRaw>>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<Ackable<AnnouncementRaw>> target)
        {
            throw new NotImplementedException();
        }

        bool ISourceBlock<Ackable<AnnouncementRaw>>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<Ackable<AnnouncementRaw>> target)
        {
            throw new NotImplementedException();
        }
    }
}
