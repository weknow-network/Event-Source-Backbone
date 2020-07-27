using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using static System.Threading.Tasks.ValueTaskStatic;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Base class for acknowledge-trigger.
    /// </summary>
    public abstract class ConsumerContext : IAck
    {
        private readonly CancellationTokenSource _ackByTimeout = new CancellationTokenSource();
        private int _onlyonce = 0;

        public ConsumerContext(Metadata metadata)
        {
            _ackByTimeout.Token.Register(() => (this as IAck).AckAsync());
            Metadata = metadata;
        }

        public Metadata Metadata { get; }

        public void AckAfter(TimeSpan timeout) => _ackByTimeout.CancelAfter(timeout);

        public abstract  ValueTask OnAckAsync();

        ValueTask IAck.AckAsync()
        {
            int once = Interlocked.Increment(ref _onlyonce);
            if (once != 1)
                return CompletedValueTask;
            return OnAckAsync();
        }
    }
}
