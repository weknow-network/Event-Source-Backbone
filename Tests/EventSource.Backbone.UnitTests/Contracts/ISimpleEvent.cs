﻿using System.ComponentModel;

namespace EventSource.Backbone.UnitTests.Entities
{

    /// <summary>
    /// Test contract
    /// </summary>
    [GenerateEventSource(EventSourceGenType.Producer)]
    [GenerateEventSource(EventSourceGenType.Consumer)]
    [Obsolete("This interface is base for code generation, please use ISimpleEventProducer or ISimpleEventConsumer", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ISimpleEvent
    {
        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        ValueTask ExecuteAsync(string key, int value);
        /// <summary>
        /// Runs the async.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="date">The date.</param>
        /// <returns>A ValueTask.</returns>
        ValueTask RunAsync(int id, DateTime date);
    }
}
