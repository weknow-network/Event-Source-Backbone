﻿namespace EventSource.Backbone.UnitTests.Entities
{
    [GenerateEventSource(EventSourceGenType.Consumer)]
    public interface IEventFlowStage1
    {
        /// <summary>
        /// Stages 1.
        /// </summary>
        /// <param name="PII">The PII.</param>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        ValueTask Stage1Async(Person PII, string payload);
    }

}
