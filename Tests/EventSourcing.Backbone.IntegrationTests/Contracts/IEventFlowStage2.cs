using System.Text.Json;

namespace EventSourcing.Backbone.UnitTests.Entities
{
    /// <summary>
    /// use to test consumer which have partial operation from IEventFlow
    /// </summary>
    [EventsContract(EventsContractType.Consumer)]
    public interface IEventFlowStage2
    {
        /// <summary>
        /// Stages the 2.
        /// </summary>
        /// <param name="PII">The PII.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        ValueTask Stage2Async(Person PII, JsonElement data);
    }

}
