namespace EventSourcing.Backbone.UnitTests.Entities
{
    [EventsContract(EventsContractType.Consumer)]
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
