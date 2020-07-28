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


        #region Context

        private static AsyncLocal<ConsumerContext?> AsyncContext = new AsyncLocal<ConsumerContext?>();

        public static ConsumerContext? Current => AsyncContext.Value;
        public static void SetContext(ConsumerContext ctx) =>
                            AsyncContext.Value = ctx;


        #endregion // Context

        public ConsumerContext(
            Metadata metadata,
            ushort? retries =  null)
        {
            _ackByTimeout.Token.Register(() => (this as IAck).AckAsync());
            Metadata = metadata;
            _retries = retries;
        }

        public Metadata Metadata { get; }

        #region Retries

        private ushort? _retries;
        /// <summary>
        /// Gets the retries time of re-consuming the message.
        /// </summary>
        public ushort? Retries
        {
            get => _retries;
            [Obsolete("Exposed for the serializer", true)]
            set => _retries = value;
        }

        #endregion Retries 


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
