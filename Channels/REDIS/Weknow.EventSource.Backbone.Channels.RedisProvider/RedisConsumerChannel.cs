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

        Ackable<Announcement> ISourceBlock<Ackable<Announcement>>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<Ackable<Announcement>> target, out bool messageConsumed)
        {
            throw new NotImplementedException();
        }

        void IDataflowBlock.Fault(Exception exception)
        {
            throw new NotImplementedException();
        }

        IDisposable ISourceBlock<Ackable<Announcement>>.LinkTo(ITargetBlock<Ackable<Announcement>> target, DataflowLinkOptions linkOptions)
        {
            throw new NotImplementedException();
        }

        void ISourceBlock<Ackable<Announcement>>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<Ackable<Announcement>> target)
        {
            throw new NotImplementedException();
        }

        bool ISourceBlock<Ackable<Announcement>>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<Ackable<Announcement>> target)
        {
            throw new NotImplementedException();
        }
    }
}
