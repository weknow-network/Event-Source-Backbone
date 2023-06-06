using System.Threading.Tasks.Dataflow;

namespace EventSourcing.Backbone.UnitTests.Entities
{
    public class SequenceOperationsConsumer : ISequenceOperationsConsumer
    {
        private readonly ActionBlock<User> _block = new ActionBlock<User>(
                            u => Console.WriteLine(u.Details.Id),
                            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 10 });
        public ValueTask RegisterAsync(ConsumerMetadata meta, User user)
        {
            //var msg = new Ackable<User>(user, ack);
            //_block.Post(msg);
            return ValueTask.CompletedTask;
        }

        public ValueTask UpdateAsync(ConsumerMetadata meta, User user) => throw new NotImplementedException();
        public ValueTask LoginAsync(ConsumerMetadata meta, string email, string password) => throw new NotImplementedException();
        public ValueTask LogoffAsync(ConsumerMetadata meta, int id) => throw new NotImplementedException();
        public ValueTask ApproveAsync(ConsumerMetadata meta, int id) => throw new NotImplementedException();
        public ValueTask SuspendAsync(ConsumerMetadata meta, int id) => throw new NotImplementedException();
        public ValueTask ActivateAsync(ConsumerMetadata meta, int id) => throw new NotImplementedException();
        public ValueTask EarseAsync(ConsumerMetadata meta, int id) => throw new NotImplementedException();
    }
}
