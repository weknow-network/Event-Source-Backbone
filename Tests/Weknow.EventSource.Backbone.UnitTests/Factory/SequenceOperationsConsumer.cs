using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using static System.Threading.Tasks.ValueTaskStatic;

namespace Weknow.EventSource.Backbone.UnitTests.Entities
{
    public class SequenceOperationsConsumer : ISequenceOperationsConsumer
    {
        private ActionBlock<Ackable<User>> _block = new ActionBlock<Ackable<User>>(
                            u => Console.WriteLine(u.Item.Details.Id),
                            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 10 });
        public ValueTask RegisterAsync(User user, IAck ack)
        {
            var msg = new Ackable<User>(user, ack);
            _block.Post(msg);
            return CompletedValueTask;
        }

        public ValueTask UpdateAsync(User user) => throw new NotImplementedException();
        public ValueTask LoginAsync(string email, string password, IAck ack ) => throw new NotImplementedException();
        public ValueTask LogoffAsync(int id) => throw new NotImplementedException();
        public ValueTask ApproveAsync(int id) => throw new NotImplementedException();
        public ValueTask SuspendAsync(int id) => throw new NotImplementedException();
        public ValueTask ActivateAsync(int id) => throw new NotImplementedException();
        public ValueTask EarseAsync(int id) => throw new NotImplementedException();
    }
}
