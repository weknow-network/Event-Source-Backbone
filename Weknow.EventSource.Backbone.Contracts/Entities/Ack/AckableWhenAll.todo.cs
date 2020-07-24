//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Threading.Tasks.Sources;

//namespace Weknow.EventSource.Backbone
//{
//    /// <summary>
//    /// <![CDATA[Enable to split event's acknowledge into multiple  
//    /// Ackable<T> with reference counting behavior 
//    /// which will ignite the original ack only when all 
//    /// split Ackable<T> produce ack or timed-out]]>  
//    /// </summary>
//    [DebuggerDisplay("DespatchMessage: {MessageId}")]
//    public class AckableWhenAll<T>
//    {
//        private static readonly ValueTask COMPLETED = new ValueTask(Task.CompletedTask);
       
//        private readonly IAck _ack;
//        private int _counter = 0;
//        // each child acknowledge entered to the queue
//        private readonly ConcurrentQueue<T> _acksBy = new ConcurrentQueue<T>();

//        #region Ctor

//        /// <summary>
//        /// Initializes a new instance.
//        /// </summary>
//        /// <param name="ack">The acknowledge handle (callback).</param>
//        public AckableWhenAll(
//            IAck ack)
//        {
//            _ack = ack;
//        }

//        #endregion // Ctor

//        #region Item

//        /// <summary>
//        /// Gets the item data.
//        /// </summary>
//        public Ackable<T> AddItem(T item)
//        {
//            Interlocked.Increment(ref _counter);
//            return new Ackable<T>(item, AckCallbackAsync);
//        }

//        #endregion // Item

//        #region AckCallbackAsync

//        /// <summary>
//        /// Acknowledge callback.
//        /// </summary>
//        /// <returns></returns>
//        private ValueTask AckCallbackAsync()
//        {
//            var count = Interlocked.Decrement(ref _counter);
//            if (count == 0)
//                _ack();
//            return COMPLETED;
//        }

//        #endregion // AckCallbackAsync
//    }
//}
