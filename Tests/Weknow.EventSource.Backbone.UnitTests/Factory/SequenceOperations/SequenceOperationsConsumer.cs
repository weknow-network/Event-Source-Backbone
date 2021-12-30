using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Weknow.EventSource.Backbone.UnitTests.Entities
{
    public class SequenceOperationsConsumer : ISequenceOperationsConsumer
    {
        private ActionBlock<User> _block = new ActionBlock<User>(
                            u => Console.WriteLine(u.Details.Id),
                            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 10 });
        public ValueTask RegisterAsync(User user)
        {
            //var msg = new Ackable<User>(user, ack);
            //_block.Post(msg);
            return ValueTask.CompletedTask;
        }

        public ValueTask UpdateAsync(User user) => throw new NotImplementedException();
        public ValueTask LoginAsync(string email, string password) => throw new NotImplementedException();
        public ValueTask LogoffAsync(int id) => throw new NotImplementedException();
        public ValueTask ApproveAsync(int id) => throw new NotImplementedException();
        public ValueTask SuspendAsync(int id) => throw new NotImplementedException();
        public ValueTask ActivateAsync(int id) => throw new NotImplementedException();
        public ValueTask EarseAsync(int id) => throw new NotImplementedException();
    }
}
