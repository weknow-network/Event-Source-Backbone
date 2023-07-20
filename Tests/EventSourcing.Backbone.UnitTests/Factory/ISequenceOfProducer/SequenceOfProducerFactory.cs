namespace EventSourcing.Backbone.UnitTests.Entities
{

    public class SequenceOfProducerFactory : ProducerPipeline, ISequenceOfProducer
    {
        public SequenceOfProducerFactory(IProducerPlan plan) : base(plan)
        {
        }

        async ValueTask<EventKeys> ISequenceOfProducer.ActivateAsync(int id)
        {
            var operation = nameof(ISequenceOfProducer.ActivateAsync);
            var classification = CreateClassificationAdapter(operation, nameof(id), id);
            return await SendAsync(operation, 0, classification);
        }

        async ValueTask<EventKeys> ISequenceOfProducer.ApproveAsync(int id)
        {
            var operation = nameof(ISequenceOfProducer.ApproveAsync);
            var classification = CreateClassificationAdapter(operation, nameof(id), id);
            return await SendAsync(operation, 0, classification);
        }

        async ValueTask<EventKeys> ISequenceOfProducer.EarseAsync(int id)
        {
            var operation = nameof(ISequenceOfProducer.EarseAsync);
            var classification = CreateClassificationAdapter(operation, nameof(id), id);
            return await SendAsync(operation, 0, classification);
        }

        async ValueTask<EventKeys> ISequenceOfProducer.LoginAsync(string email, string password)
        {
            var operation = nameof(ISequenceOfProducer.LoginAsync);
            var classification0 = CreateClassificationAdapter(operation, nameof(email), email);
            var classification1 = CreateClassificationAdapter(operation, nameof(password), password);
            return await SendAsync(operation, 0, classification0, classification1);
        }

        async ValueTask<EventKeys> ISequenceOfProducer.LogoffAsync(int id)
        {
            var operation = nameof(ISequenceOfProducer.ApproveAsync);
            var classification = CreateClassificationAdapter(operation, nameof(id), id);
            return await SendAsync(operation, 0, classification);
        }

        async ValueTask<EventKeys> ISequenceOfProducer.RegisterAsync(User user)
        {
            var operation = nameof(ISequenceOfProducer.RegisterAsync);
            var classification = CreateClassificationAdapter(operation, nameof(user), user);
            return await SendAsync(operation, 0, classification);
        }

        async ValueTask<EventKeys> ISequenceOfProducer.SuspendAsync(int id)
        {
            var operation = nameof(ISequenceOfProducer.SuspendAsync);
            var classification = CreateClassificationAdapter(operation, nameof(id), id);
            return await SendAsync(operation, 0, classification);
        }

        async ValueTask<EventKeys> ISequenceOfProducer.UpdateAsync(User user)
        {
            var operation = nameof(ISequenceOfProducer.UpdateAsync);
            var classification = CreateClassificationAdapter(operation, nameof(user), user);
            return await SendAsync(operation, 0, classification);
        }
    }
}
