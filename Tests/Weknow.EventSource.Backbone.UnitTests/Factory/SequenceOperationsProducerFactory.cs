using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.UnitTests.Entities;

namespace Weknow.EventSource.Backbone.UnitTests
{
    public class SequenceOperationsProducerFactory : ProducerPipeline, ISequenceOperationsProducer
    {
        private readonly ProducerPipeline _pipeline;

        public SequenceOperationsProducerFactory(IProducerPlan plan) : base(plan)
        {
        }

        async ValueTask ISequenceOperationsProducer.ActivateAsync(int id)
        {
            var operation = nameof(ISequenceOperationsProducer.ActivateAsync);
            var classification = CreateClassificationAdaptor(operation, nameof(id), id);
            await SendAsync(operation, classification);
        }

        async ValueTask ISequenceOperationsProducer.ApproveAsync(int id)
        {
            var operation = nameof(ISequenceOperationsProducer.ApproveAsync);
            var classification = CreateClassificationAdaptor(operation, nameof(id), id);
            await SendAsync(operation, classification);
        }

        async ValueTask ISequenceOperationsProducer.EarseAsync(int id)
        {
            var operation = nameof(ISequenceOperationsProducer.EarseAsync);
            var classification = CreateClassificationAdaptor(operation, nameof(id), id);
            await SendAsync(operation, classification);
        }

        async ValueTask ISequenceOperationsProducer.LoginAsync(string email, string password)
        {
            var operation = nameof(ISequenceOperationsProducer.LoginAsync);
            var classification0 = CreateClassificationAdaptor(operation, nameof(email), email);
            var classification1 = CreateClassificationAdaptor(operation, nameof(password), password);
            await SendAsync(operation, classification0, classification1);
        }

        async ValueTask ISequenceOperationsProducer.LogoffAsync(int id)
        {
            var operation = nameof(ISequenceOperationsProducer.ApproveAsync);
            var classification = CreateClassificationAdaptor(operation, nameof(id), id);
            await SendAsync(operation, classification);
        }

        async ValueTask ISequenceOperationsProducer.RegisterAsync(User user)
        {
            var operation = nameof(ISequenceOperationsProducer.RegisterAsync);
            var classification = CreateClassificationAdaptor(operation, nameof(user), user);
            await SendAsync(operation, classification);
        }

        async ValueTask ISequenceOperationsProducer.SuspendAsync(int id)
        {
            var operation = nameof(ISequenceOperationsProducer.SuspendAsync);
            var classification = CreateClassificationAdaptor(operation, nameof(id), id);
            await SendAsync(operation, classification);
        }

        async ValueTask ISequenceOperationsProducer.UpdateAsync(User user)
        {
            var operation = nameof(ISequenceOperationsProducer.UpdateAsync);
            var classification = CreateClassificationAdaptor(operation, nameof(user), user);
            await SendAsync(operation, classification);
        }
    }
}
