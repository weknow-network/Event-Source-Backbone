﻿using System;

namespace Weknow.EventSource.Backbone
{
    public record ConsumerOptions :
        EventSourceOptions
    {
        //public new static readonly ConsumerOptions Empty = new ConsumerOptions();

        #region BatchSize

        /// <summary>
        /// Gets the max batch size of reading messages per shard.
        /// The framework won't proceed to the next batch until all messages
        /// in the batch complete (or timeout when it set to acknowledge on timeout).
        /// </summary>
        public int BatchSize { get; init; } = 100;

        #endregion // BatchSize

        #region KeepAlive

        /// <summary>
        /// Gets a value indicating whether to prevent the consumer
        /// from being collect by the GC.
        /// True by default, when you hold the subscription disposable
        /// you can set it to false. as long as you keeping the disposable in
        /// object that isn't candidate for being collected the consumer will stay alive.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [keep alive]; otherwise, <c>false</c>.
        /// </value>
        public bool KeepAlive { get; init; } = true;

        #endregion // Validation

        #region AckBehavior

        /// <summary>
        /// Gets the acknowledge behavior.
        /// </summary>
        public AckBehavior AckBehavior { get; init; } = AckBehavior.OnSucceed;

        #endregion AckBehavior 

        #region MaxMessages

        /// <summary>
        /// Gets the maximum messages to consume before detaching the subscription.
        /// any number > 0 will activate this mechanism.
        /// </summary>
        public uint MaxMessages { get; init; } = 0;

        #endregion MaxMessages 

        #region TraceAsParent

        /// <summary>
        /// Gets the threshold duration which limit considering producer call as parent,
        /// Beyond this period the consumer span will refer the producer as Link (rather than parent).
        /// </summary>
        public TimeSpan TraceAsParent { get; init; }

        #endregion // TraceAsParent
    }
}