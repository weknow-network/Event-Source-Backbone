namespace EventSourcing.Backbone.UnitTests.Entities
{

    public class SequenceOperationsProducerFactory : ProducerPipeline, ISequenceOperationsProducer
    {
        public SequenceOperationsProducerFactory(IProducerPlan plan) : base(plan)
        {
        }

        async ValueTask<EventKeys> ISequenceOperationsProducer.ActivateAsync(int id)
        {
            var operation = nameof(ISequenceOperationsProducer.ActivateAsync);
            var classification = CreateClassificationAdapter(operation, nameof(id), id);
            return await SendAsync(operation, 0, classification);
        }

        async ValueTask<EventKeys> ISequenceOperationsProducer.ApproveAsync(int id)
        {
            var operation = nameof(ISequenceOperationsProducer.ApproveAsync);
            var classification = CreateClassificationAdapter(operation, nameof(id), id);
            return await SendAsync(operation, 0, classification);
        }

        async ValueTask<EventKeys> ISequenceOperationsProducer.EarseAsync(int id)
        {
            var operation = nameof(ISequenceOperationsProducer.EarseAsync);
            var classification = CreateClassificationAdapter(operation, nameof(id), id);
            return await SendAsync(operation, 0, classification);
        }

        async ValueTask<EventKeys> ISequenceOperationsProducer.LoginAsync(string email, string password)
        {
            var operation = nameof(ISequenceOperationsProducer.LoginAsync);
            var classification0 = CreateClassificationAdapter(operation, nameof(email), email);
            var classification1 = CreateClassificationAdapter(operation, nameof(password), password);
            return await SendAsync(operation, 0, classification0, classification1);
        }

        async ValueTask<EventKeys> ISequenceOperationsProducer.LogoffAsync(int id)
        {
            var operation = nameof(ISequenceOperationsProducer.ApproveAsync);
            var classification = CreateClassificationAdapter(operation, nameof(id), id);
            return await SendAsync(operation, 0, classification);
        }

        async ValueTask<EventKeys> ISequenceOperationsProducer.RegisterAsync(User user)
        {
            var operation = nameof(ISequenceOperationsProducer.RegisterAsync);
            var classification = CreateClassificationAdapter(operation, nameof(user), user);
            return await SendAsync(operation, 0, classification);
        }

        async ValueTask<EventKeys> ISequenceOperationsProducer.SuspendAsync(int id)
        {
            var operation = nameof(ISequenceOperationsProducer.SuspendAsync);
            var classification = CreateClassificationAdapter(operation, nameof(id), id);
            return await SendAsync(operation, 0, classification);
        }

        async ValueTask<EventKeys> ISequenceOperationsProducer.UpdateAsync(User user)
        {
            var operation = nameof(ISequenceOperationsProducer.UpdateAsync);
            var classification = CreateClassificationAdapter(operation, nameof(user), user);
            return await SendAsync(operation, 0, classification);
        }
    }
}
