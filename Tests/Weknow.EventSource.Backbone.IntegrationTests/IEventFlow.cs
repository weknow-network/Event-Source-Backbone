﻿using System.Text.Json;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.UnitTests.Entities
{

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
