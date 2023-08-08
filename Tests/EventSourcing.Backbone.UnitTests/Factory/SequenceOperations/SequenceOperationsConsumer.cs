using System.Threading.Tasks.Dataflow;

namespace EventSourcing.Backbone.UnitTests.Entities
{
    public class SequenceOperationsConsumer : ISequenceOfConsumer
    {
        private readonly ActionBlock<User> _block = new ActionBlock<User>(
                            u => Console.WriteLine(u.Details.Id),
                            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 10 });
        public ValueTask RegisterAsync(ConsumerContext meta, User user)
        {
            //var msg = new Ackable<User>(user, ack);
            //_block.Post(msg);
            return ValueTask.CompletedTask;
        }

        public ValueTask UpdateAsync(ConsumerContext meta, User user) => throw new NotImplementedException();
        public ValueTask LoginAsync(ConsumerContext meta, string email, string password) => throw new NotImplementedException();
        public ValueTask LogoffAsync(ConsumerContext meta, int id) => throw new NotImplementedException();
        public ValueTask ApproveAsync(ConsumerContext meta, int id) => throw new NotImplementedException();
        public ValueTask SuspendAsync(ConsumerContext meta, int id) => throw new NotImplementedException();
        public ValueTask ActivateAsync(ConsumerContext meta, int id) => throw new NotImplementedException();
        public ValueTask EarseAsync(ConsumerContext meta, int id) => throw new NotImplementedException();
    }
}
