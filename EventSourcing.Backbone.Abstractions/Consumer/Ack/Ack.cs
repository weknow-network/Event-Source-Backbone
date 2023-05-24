namespace EventSourcing.Backbone
{
    /// <summary>
    /// Acknowledge context
    /// </summary>
    /// <seealso cref="System.IAsyncDisposable" />
    public static class Ack
    {
        private static readonly AsyncLocal<IAck> _context = new AsyncLocal<IAck>();
        public static readonly IAck Empty = NOP.Default;

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="Ack"/> class.
        /// </summary>
        static Ack()
        {
            _context.Value = NOP.Default;
        }

        #endregion // Ctor

        #region Set

        /// <summary>
        /// Sets ack.
        /// </summary>
        /// <param name="ack"></param>
        /// <returns></returns>
        public static IAsyncDisposable Set(IAck ack)
        {
            return new Scope(ack);
        }

        #endregion // Set

        #region Current

        /// <summary>
        /// Gets the current.
        /// </summary>
        public static IAck Current => _context.Value ?? throw new NullReferenceException("IAck Current");

        #endregion // Current

        #region NOP

        /// <summary>
        /// Empty implementation
        /// </summary>
        /// <seealso cref="EventSourcing.Backbone.IAck" />
        private readonly struct NOP : IAck
        {
            public static readonly IAck Default = new NOP();
            /// <summary>
            /// Preform acknowledge (which should prevent the
            /// message from process again by the consumer)
            /// </summary>
            /// <returns></returns>
            public ValueTask AckAsync() => ValueTask.CompletedTask;

            /// <summary>
            /// Cancel acknowledge (will happen on error in order to avoid ack on succeed)
            /// </summary>
            public ValueTask CancelAsync() => ValueTask.CompletedTask;

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
            /// </summary>
            /// <returns>
            /// A task that represents the asynchronous dispose operation.
            /// </returns>
            public ValueTask DisposeAsync()
            {
                return ValueTask.CompletedTask;
            }
        }

        #endregion // NOP

        #region Scope

        /// <summary>
        /// Disposable scope
        /// </summary>
        /// <seealso cref="System.IAsyncDisposable" />
        private struct Scope : IAsyncDisposable
        {
            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="ack">The ack.</param>
            public Scope(IAck ack)
            {
                _context.Value = ack;
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
            /// </summary>
            /// <returns>
            /// A task that represents the asynchronous dispose operation.
            /// </returns>
            public ValueTask DisposeAsync()
            {
                // the Ack.Current won't be available any more after closing this scope
                // invocation is over and cannot Ack.Current.AckAsync() anymore.
                _context.Value = NOP.Default;
                return ValueTask.CompletedTask;
            }
        }

        #endregion // Scope
    }
}