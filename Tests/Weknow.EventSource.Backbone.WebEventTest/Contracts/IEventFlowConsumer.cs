﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.WebEventTest
{
    /// <summary>
    /// Consumer contract
    /// </summary>
    //[GenerateEventSourceContract(EventSourceGenType.Producer)]
    [GenerateEventSourceContract(EventSourceGenType.Producer)]
    public interface IEventFlowConsumer
    {
        /// <summary>
        /// Stages 1.
        /// </summary>
        /// <param name="PII">The PII.</param>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        void Stage1Async(Person PII, string payload, string key);
        /// <summary>
        /// Stages the 2.
        /// </summary>
        /// <param name="PII">The PII.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        ValueTask Stage2Async(JsonElement PII, JsonElement data);
    }
}