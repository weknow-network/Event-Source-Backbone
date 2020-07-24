using System;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Represent acknowledge-able instance.
    /// </summary>
    public class Ackable<T>
    {
        #region Ctor

        /// <summary>
        /// Prevents a default instance.
        /// </summary>
        [Obsolete("Use other constructors (this one exists to enable de-serialization)", true)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        private Ackable()
        {
        }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// Initializes a new instance        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="ack">The acknowledge handle (callback).</param>
        public Ackable(
            T item,
            IAck ack)
        {
            Item = item;
            _ack = ack;
        }

        #endregion // Ctor

        #region Item

        /// <summary>
        /// Gets the item data.
        /// </summary>
        public T Item { get; }

        #endregion // Item

        #region Ack

        private IAck _ack;
        /// <summary>
        /// Send Acknowledge (some queue type [like event sourcing] 
        /// keep the item in the queue until it processed,
        /// the acknowledge will notify it that it can be delete).
        /// </summary>
        /// <returns></returns>
        public IAck Ack
        {
            get => _ack;
            [Obsolete("Exposed for the serializer", true)]
            set => _ack = value;
        }

        #endregion Ack 
    }
}
