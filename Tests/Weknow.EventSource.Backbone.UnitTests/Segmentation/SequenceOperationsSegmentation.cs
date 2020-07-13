using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.UnitTests.Entities
{
    public class SequenceOperationsSegmentation: ISequenceOperations
    {
        private readonly EventSourceOptions _options;
        private readonly Action<string, ReadOnlyMemory<byte>> _store;
        private static readonly ValueTask COMPLETED = new ValueTask();
        
        public SequenceOperationsSegmentation(
                    EventSourceOptions options,
                    Action<string, ReadOnlyMemory<byte>> store)
        {
            _options = options;
            _store = store;
        }

        ValueTask ISequenceOperations.RegisterAsync(User user)
        {
            var personal = _options.Serializer.Serialize(user.Eracure);
            var open = _options.Serializer.Serialize(user.Details);

            _store(nameof(personal), personal);
            _store(nameof(open), open);
            return COMPLETED;
        }
        ValueTask ISequenceOperations.UpdateAsync(User user)
        { 
            var personal = _options.Serializer.Serialize(user.Eracure);
            var open = _options.Serializer.Serialize(user.Details);

            _store(nameof(personal), personal);
            _store(nameof(open), open);
            return COMPLETED;
        }
        ValueTask ISequenceOperations.LoginAsync(
            string email,
            string password)
        {
            var e = _options.Serializer.Serialize(email);
            var p = _options.Serializer.Serialize(password);
            _store(nameof(email), e);
            _store(nameof(password), p);
            return COMPLETED;
        }

        ValueTask ISequenceOperations.LogoffAsync(int id) => Store(id);
        ValueTask ISequenceOperations.ApproveAsync(int id) => Store(id);
        ValueTask ISequenceOperations.SuspendAsync(int id) => Store(id);
        ValueTask ISequenceOperations.ActivateAsync(int id) => Store(id);
        ValueTask ISequenceOperations.EarseAsync(int id) => Store(id);

        private ValueTask Store(int id)
        {
            var i = _options.Serializer.Serialize(id);
            _store(nameof(id), i);
            return COMPLETED;
        }
    }
}
