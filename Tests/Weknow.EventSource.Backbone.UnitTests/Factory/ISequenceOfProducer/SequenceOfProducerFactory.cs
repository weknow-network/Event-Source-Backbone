using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.UnitTests.Entities;

namespace Weknow.EventSource.Backbone.UnitTests.Entities
{

    public class SequenceOfProducerFactory : ProducerPipeline, ISequenceOfProducer
    {
        public SequenceOfProducerFactory(IProducerPlan plan) : base(plan)
        {
        }

        async ValueTask<EventKey[]> ISequenceOfProducer.ActivateAsync(int id)
        {
            var operation = nameof(ISequenceOfProducer.ActivateAsync);
            var classification = CreateClassificationAdaptor(operation, nameof(id), id);
            return await SendAsync(operation, classification);
        }

        async ValueTask<EventKey[]> ISequenceOfProducer.ApproveAsync(int id)
        {
            var operation = nameof(ISequenceOfProducer.ApproveAsync);
            var classification = CreateClassificationAdaptor(operation, nameof(id), id);
            return await SendAsync(operation, classification);
        }

        async ValueTask<EventKey[]> ISequenceOfProducer.EarseAsync(int id)
        {
            var operation = nameof(ISequenceOfProducer.EarseAsync);
            var classification = CreateClassificationAdaptor(operation, nameof(id), id);
            return await SendAsync(operation, classification);
        }

        async ValueTask<EventKey[]> ISequenceOfProducer.LoginAsync(string email, string password)
        {
            var operation = nameof(ISequenceOfProducer.LoginAsync);
            var classification0 = CreateClassificationAdaptor(operation, nameof(email), email);
            var classification1 = CreateClassificationAdaptor(operation, nameof(password), password);
            return await SendAsync(operation, classification0, classification1);
        }

        async ValueTask<EventKey[]> ISequenceOfProducer.LogoffAsync(int id)
        {
            var operation = nameof(ISequenceOfProducer.ApproveAsync);
            var classification = CreateClassificationAdaptor(operation, nameof(id), id);
            return await SendAsync(operation, classification);
        }

        async ValueTask<EventKey[]> ISequenceOfProducer.RegisterAsync(User user)
        {
            var operation = nameof(ISequenceOfProducer.RegisterAsync);
            var classification = CreateClassificationAdaptor(operation, nameof(user), user);
            return await SendAsync(operation, classification);
        }

        async ValueTask<EventKey[]> ISequenceOfProducer.SuspendAsync(int id)
        {
            var operation = nameof(ISequenceOfProducer.SuspendAsync);
            var classification = CreateClassificationAdaptor(operation, nameof(id), id);
            return await SendAsync(operation, classification);
        }

        async ValueTask<EventKey[]> ISequenceOfProducer.UpdateAsync(User user)
        {
            var operation = nameof(ISequenceOfProducer.UpdateAsync);
            var classification = CreateClassificationAdaptor(operation, nameof(user), user);
            return await SendAsync(operation, classification);
        }
    }
}
