﻿using System.ComponentModel;
using System.Text.Json;

namespace EventSourcing.Backbone.WebEventTest
{
    [EventsContract(EventsContractType.Consumer)]
    [EventsContract(EventsContractType.Producer)]
    //[EventsContract(EventSourceGenType.Consumer, Namespace = "EventSourcing.Backbone.WebEventTest")]
    //[EventsContract(EventSourceGenType.Producer, Namespace = "EventSourcing.Backbone.WebEventTest")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Used for code generation, use the producer / consumer version of it", true)]
    public interface IEventFlow
    {
        /// <summary>
        /// Stages 1.
        /// </summary>
        /// <param name="PII">The PII.</param>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        ValueTask Stage1Async(Person PII, string payload);
        /// <summary>
        /// Stages the 2.
        /// </summary>
        /// <param name="PII">The PII.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        ValueTask Stage2Async(JsonElement PII, JsonElement data);
    }
}
